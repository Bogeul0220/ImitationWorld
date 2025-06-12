using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneColliderGenerator : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedMeshRenderer;

    [Header("자동으로 콜라이더를 붙일 본 이름 키워드")]
    public List<string> BoxColliderKeyords = new();
    public List<string> CapsuleColliderKeywords = new();
    public List<string> SphereColliderKeywords = new();

    public float MinSize = 1f;
    public float RadiusMultiplier = 0.2f;

    [ContextMenu("콜라이더 자동 생성")]
    public void GenerateCollider()
    {
        if (SkinnedMeshRenderer == null)
            return;

        foreach (Transform bone in SkinnedMeshRenderer.bones)
        {
            string name = bone.name.ToLower();

            if (HasKeyword(name, BoxColliderKeyords))
                CreateBoxCollider(bone);
            else if (HasKeyword(name, CapsuleColliderKeywords))
                CreateCapsuleCollider(bone);
            else if (HasKeyword(name, SphereColliderKeywords))
                CreateSphereCollider(bone);
        }
    }

    private bool HasKeyword(string name, List<string> keywords)
    {
        foreach (string keyword in keywords)
            if (name.Contains(keyword.ToLower()))
                return true;

        return false;
    }

    private void CreateBoxCollider(Transform bone)
    {
        var col = bone.gameObject.AddComponent<BoxCollider>();

        Vector3 scale = bone.lossyScale;
        col.size = new Vector3(
            Mathf.Max(MinSize, scale.x),
            Mathf.Max(MinSize, scale.y),
            Mathf.Max(MinSize, scale.z)
        );
    }

    private void CreateCapsuleCollider(Transform bone)
    {
        var col = bone.gameObject.AddComponent<CapsuleCollider>();

        Transform child = GetFirstChild(bone);
        if (child != null)
        {
            Vector3 dir = child.position - bone.position;
            float length = dir.magnitude;

            int direction = GetDominantDirection(dir.normalized);
            col.direction = direction;
            col.height = Mathf.Max(MinSize * 2, length);
            col.radius = Mathf.Max(MinSize, length * RadiusMultiplier);
        }
        else
        {
            col.height = 0.5f;
            col.radius = 0.1f;
            col.direction = 1;
        }
    }

    private void CreateSphereCollider(Transform bone)
    {
        var col = bone.gameObject.AddComponent<SphereCollider>();
        Vector3 scale = bone.lossyScale;
        col.radius = Mathf.Max(MinSize, (scale.x + scale.y + scale.z) / 6f);
    }

    private Transform GetFirstChild(Transform bone)
    {
        if (bone.childCount > 0)
            return bone.GetChild(0);
        return null;
    }

    private int GetDominantDirection(Vector3 dir)
    {
        dir = dir.normalized;
        float x = Mathf.Abs(dir.x);
        float y = Mathf.Abs(dir.y);
        float z = Mathf.Abs(dir.z);

        if (y >= x && y >= z) return 1; // Y
        if (x >= y && x >= z) return 0; // X
        return 2; // Z
    }
}
