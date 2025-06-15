using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    public Terrain[] terrains;
    [SerializeField] private EnvironmentObject treePrefab;
    [SerializeField] private EnvironmentObject rockPrefab;
    [SerializeField] private int spawnCount;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        SpawnEnvironmentObject(treePrefab);
        SpawnEnvironmentObject(rockPrefab);
    }

    private void SpawnEnvironmentObject(EnvironmentObject obj)
    {
        GameObject objParents = new GameObject();
        objParents.name = $"{obj.name}Parents";
        objParents.transform.parent = this.transform;

        for (int i = 0; i < spawnCount; i++)
        {
            int terrainIndex = Random.Range(0, terrains.Length);
            Terrain selected = terrains[terrainIndex];

            Vector3 pos = GetRandomPositionTerrains(selected);
            var newObject = ObjectPoolManager.Get<EnvironmentObject>(obj.gameObject);
            SetObjectOnTerrain(selected, newObject.gameObject, pos);

            newObject.transform.SetParent(objParents.transform);
            newObject.OnInit();
        }
    }

    private Vector3 GetRandomPositionTerrains(Terrain selected)
    {
        // Terrain 선택
        TerrainData data = selected.terrainData;
        Vector3 terrainPos = selected.transform.position;

        float terrainWidth = data.size.x;
        float terrainLength = data.size.z;

        // Terrain 내부 랜덤 좌표 뽑기
        float localX = Random.Range(0f, terrainWidth);
        float localZ = Random.Range(0f, terrainLength);

        float worldX = terrainPos.x + localX;
        float worldZ = terrainPos.z + localZ;
        float worldY = selected.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrainPos.y;

        return new Vector3(worldX, worldY, worldZ);
    }

    private void SetObjectOnTerrain(Terrain terrain, GameObject obj, Vector3 _position)
    {
        // 높이 맞추기
        float height = terrain.SampleHeight(_position) + terrain.GetPosition().y;
        _position.y = height;
        obj.transform.position = _position;

        // 노멀 구하기 (0~1로 정규화된 좌표)
        Vector3 normalizedPos = new Vector3(
            (_position.x - terrain.GetPosition().x / terrain.terrainData.size.x),
            0,
            (_position.z - terrain.GetPosition().z / terrain.terrainData.size.z)
        );

        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.z);

        // 회전 적용 - 노멀을 업벡터로
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        obj.transform.rotation = rotation;
    }
}
