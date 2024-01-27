using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private Transform carrotSpawnP1;
    [SerializeField] private Transform carrotSpawnP2;
    [SerializeField] private List<GameObject> carrotPattern;

    private readonly List<ProjectileStateController> carrotsP1Side = new();
    private readonly List<ProjectileStateController> carrotsP2Side = new();

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        
        var indexP1 = Random.Range(0, carrotPattern.Count);
        var carrotsP1 = carrotPattern[indexP1];
        var p1 = Instantiate(carrotsP1, carrotSpawnP1.position, carrotSpawnP1.rotation, transform);

        foreach (var pro in p1.GetComponentsInChildren<ProjectileStateController>())
        {
            carrotsP1Side.Add(pro);
            pro.OnStateChanged += OnProjectileStateChanged;

            //Rotate individual carrots back so they face the right direction
            pro.transform.localEulerAngles = -carrotSpawnP1.rotation.eulerAngles;
        }

        var indexP2 = Random.Range(0, carrotPattern.Count);
        var carrotsP2 = carrotPattern[indexP2];
        var p2 = Instantiate(carrotsP2, carrotSpawnP2.position, carrotSpawnP2.rotation, transform);
        
        foreach (var pro in p2.GetComponentsInChildren<ProjectileStateController>())
        {
            carrotsP2Side.Add(pro);
            pro.OnStateChanged += OnProjectileStateChanged;
            
            //Rotate individual carrots back so they face the right direction
            pro.transform.localEulerAngles = -carrotSpawnP2.rotation.eulerAngles;
        }
    }

    private void OnProjectileStateChanged(ProjectileStateController pro, ProjectileState state)
    {
        if (state == ProjectileState.Fired)
        {
            if (carrotsP1Side.Contains(pro)) carrotsP1Side.Remove(pro);
            if (carrotsP2Side.Contains(pro)) carrotsP2Side.Remove(pro);
        }
        else if (state == ProjectileState.Grounded)
        {
            var pos = cam.WorldToViewportPoint(pro.transform.position).x;
            if (Mathf.Sign(pos * 2 - 1) < 0)
            {
                if (!carrotsP1Side.Contains(pro)) carrotsP1Side.Add(pro);
            }
            else
            {
                if (!carrotsP2Side.Contains(pro)) carrotsP2Side.Add(pro);
            }
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        if (carrotsP1Side.Count == 0 || carrotsP2Side.Count == 0)
        {
            gameController.GameOver();
        }
    }

    public GameResult GetGameResult()
    {
        if (carrotsP1Side.Count == carrotsP2Side.Count)
        {
            return GameResult.Draw;
        }
        else if (carrotsP1Side.Count > carrotsP2Side.Count)
        {
            return GameResult.Player2Wins;
        }
        else
        {
            return GameResult.Player1Wins;
        }
    }
}