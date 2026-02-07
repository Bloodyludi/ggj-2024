Shader "Hidden/BeatDebugGraph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurrentTime ("Current Time", Float) = 0
        _BeatInterval ("Beat Interval", Float) = 0.5
        _WindowFrac ("Window Fraction", Float) = 0.1
        _BeatsToShow ("Beats to Show", Float) = 6
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            #define MAX_CLICKS 16

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _CurrentTime;
            float _BeatInterval;
            float _WindowFrac;
            float _BeatsToShow;
            int _ClickCount;
            float _ClickTimes[MAX_CLICKS];

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Classic cubic smoothstep: f(0)=0, f(1)=1, f'(0)=0, f'(1)=0
            float _sstep(float x)
            {
                x = saturate(x);
                return x * x * (3.0 - 2.0 * x);
            }

            // Beat activation function
            // phase: 0-1 within one beat cycle (0 = exact beat moment)
            // w: window fraction (e.g. 0.1 for 10%)
            //
            // Shape: flat at 1 inside the window (any press is equally valid),
            //        spike to 2 at the exact beat, sharp edges at window boundaries.
            float beatFunction(float phase, float w)
            {
                float edge = 0.2; // edge transition width (fraction of window)

                // Post-beat zone: phase [0, w]
                if (phase <= w)
                {
                    float t = phase / w; // 0 at beat, 1 at window close

                    // Flat window with sharp closing edge
                    float window = 1.0 - _sstep(saturate((t - (1.0 - edge)) / edge));

                    // Beat spike: quick punch at phase 0
                    float spike = 1.0 - _sstep(saturate(t / 0.15));

                    return window + spike;
                }

                // Pre-beat zone: phase [1-w, 1]
                if (phase >= (1.0 - w))
                {
                    float t = (phase - (1.0 - w)) / w; // 0 at window open, 1 at beat

                    // Flat window with sharp opening edge
                    return _sstep(saturate(t / edge));
                }

                // Dead zone: no window active
                return 0.0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                if (_BeatInterval <= 0.001)
                    return fixed4(0.05, 0.05, 0.1, 0.88);

                // Map horizontal position to time (scrolling window, current time at right edge)
                float timeSpan = _BeatsToShow * _BeatInterval;
                float pixelTime = _CurrentTime - timeSpan * (1.0 - uv.x);

                // Pixel sizes for resolution-independent anti-aliased lines
                float pxY = fwidth(uv.y);
                float pxX = fwidth(uv.x);

                // Background gradient
                float3 col = lerp(float3(0.07, 0.07, 0.14), float3(0.04, 0.04, 0.09), uv.y);

                // Before music started: dim
                if (pixelTime < 0.0)
                    return fixed4(col * 0.5, 0.88);

                // Beat phase and activation value for this pixel's time
                float phase = frac(pixelTime / _BeatInterval);
                float value = beatFunction(phase, _WindowFrac);

                // Normalize value (0-2) into UV y-space with headroom
                float maxY = 2.3;
                float normVal = value / maxY;

                // --- Grid ---

                // Horizontal guides at value = 0, 1, 2
                float3 guideCol = float3(0.18, 0.18, 0.25);
                float g0 = 1.0 - smoothstep(0.0, pxY * 1.5, abs(uv.y));
                float g1 = 1.0 - smoothstep(0.0, pxY * 1.5, abs(uv.y - 1.0 / maxY));
                float g2 = 1.0 - smoothstep(0.0, pxY * 1.5, abs(uv.y - 2.0 / maxY));
                col = lerp(col, guideCol, max(g0, max(g1, g2)) * 0.6);

                // --- Red target zone (beat window = when to click) ---
                bool inWindow = (phase <= _WindowFrac) || (phase >= (1.0 - _WindowFrac));
                if (inWindow)
                {
                    // Red background tint across full height
                    col += float3(0.06, 0.01, 0.01);

                    // Stronger red fill under the curve
                    if (uv.y <= normVal && normVal > 0.001)
                    {
                        float fill = 0.15 * (1.0 - uv.y / normVal);
                        col += float3(0.5, 0.05, 0.02) * fill;
                    }
                }

                // Vertical beat lines - bright red (the ideal click moment)
                float beatDist = min(phase, 1.0 - phase) / _BeatsToShow;
                float beatLine = 1.0 - smoothstep(0.0, pxX * 1.5, beatDist);
                col = lerp(col, float3(0.9, 0.15, 0.1), beatLine * 0.7);

                // Window boundary markers (where the window opens/closes)
                float preBound = abs(phase - (1.0 - _WindowFrac)) / _BeatsToShow;
                float postBound = abs(phase - _WindowFrac) / _BeatsToShow;
                float boundLine = 1.0 - smoothstep(0.0, pxX * 1.5, min(preBound, postBound));
                col = lerp(col, float3(0.5, 0.12, 0.08), boundLine * 0.4);

                // --- Curve ---

                float delta = normVal - uv.y;

                // Adaptive line width: thicken at steep parts so the spike stays visible
                float curveRate = fwidth(normVal);
                float adaptiveWidth = max(pxY * 2.0, curveRate * 0.5);

                // Outer glow
                float glowWidth = max(pxY * 10.0, curveRate * 0.8);
                float glow = exp(-delta * delta / (glowWidth * glowWidth + 0.0001));
                col = lerp(col, float3(0.0, 0.3, 0.1), glow * 0.25);

                // Core curve line
                float core = exp(-delta * delta / (adaptiveWidth * adaptiveWidth + 0.0001));
                float spikeAmt = saturate(value - 1.0);
                float3 curveCol = lerp(
                    float3(0.2, 1.0, 0.45),   // green for window region
                    float3(1.0, 1.0, 0.7),    // bright yellow-white for spike
                    spikeAmt
                );
                col = lerp(col, curveCol, core);

                // Extra bright bloom at beat spike
                if (value > 1.2)
                {
                    float brightWidth = max(pxY * 1.2, curveRate * 0.3);
                    float bright = exp(-delta * delta / (brightWidth * brightWidth + 0.0001));
                    col = lerp(col, float3(1.0, 1.0, 1.0), bright * spikeAmt * 0.6);
                }

                // --- Click bars ---
                // Positive timestamp = on-beat (cyan), negative = off-beat (orange)
                // Loop bound clamped to MAX_CLICKS so the compiler can unroll
                int count = min(_ClickCount, MAX_CLICKS);
                for (int j = 0; j < MAX_CLICKS; j++)
                {
                    if (j >= count) break;

                    float raw = _ClickTimes[j];
                    float clickTime = abs(raw);

                    // Skip clicks outside the visible time range
                    float clickDist = abs(pixelTime - clickTime);
                    if (clickDist > timeSpan) continue;

                    float bar = 1.0 - smoothstep(0.0, pxX * 2.5, clickDist / timeSpan);
                    if (bar > 0.01)
                    {
                        bool onBeat = raw >= 0.0;
                        float3 barCol = onBeat
                            ? float3(0.1, 0.95, 1.0)   // cyan = on-beat
                            : float3(1.0, 0.4, 0.1);   // orange = off-beat
                        col = lerp(col, barCol, bar * 0.75);
                    }
                }

                // --- Playhead (current time at right edge) ---
                float phDist = 1.0 - uv.x;
                float playhead = 1.0 - smoothstep(0.0, pxX * 2.0, phDist);
                col = lerp(col, float3(0.6, 0.6, 0.85), playhead * 0.7);

                return fixed4(col, 0.88);
            }
            ENDCG
        }
    }
    FallBack Off
}
