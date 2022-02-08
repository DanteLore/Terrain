using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobGenerator: ChunkDecorator
{
    private Dictionary<Vector2, List<GameObject>> mobs;

    protected override void Awake()
    {
        base.Awake();

        priority = 20;
        mobs = new Dictionary<Vector2, List<GameObject>>();
    }

    public override void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        base.OnChunkVisibilityChanged(chunk, visible);
        
        /// TODO: Problem here - and below - is that mobs are being returned to the pool based on the chunk they spawned on, not on the chunk they ARE on.
        /// So the chunk based system that works for static objects isn't going to work for mobs.  Some kind of global mob management is going to be needed.

        //if(!visible && mobs.ContainsKey(chunk.coord))
        //{
        //    mobs[chunk.coord].ForEach(ReleaseToPool);
        //    mobs.Remove(chunk.coord);
        //}
    }

    public override void OnLodChange(TerrainChunk chunk, int lod)
    {
        base.OnLodChange(chunk, lod);

        if(lod <= 0 && !mobs.ContainsKey(chunk.coord))
        {
            GenerateMobs(chunk);
        }
        else if(lod > 1 && mobs.ContainsKey(chunk.coord))
        {
            mobs[chunk.coord].ForEach(ReleaseToPool);
            mobs.Remove(chunk.coord);
        }
    }

    private void GenerateMobs(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));
        mobs[chunk.coord] = new List<GameObject>();

        if(rand.Next(10) >= 5)
        {
            int x = rand.Next(chunk.MapWidth);
            int y = rand.Next(chunk.MapHeight);

            var mob = PlaceMob(chunk, x, y, rand);
            if(mob != null)
                mobs[chunk.coord].Add(mob);
        }
    }

    private GameObject PlaceMob(TerrainChunk chunk, int x, int y, System.Random rand)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        Biome biome = chunk.BlendedBiome(pos, rand);
        MobSettings mobSettings = biome.settings.mobSettings;

        GameObject prefab = mobSettings.prefabs[rand.Next(mobSettings.prefabs.Length)];
        GameObject mob = InstantiateFromPool(prefab);
        mob.transform.SetParent(chunk.meshObject.transform);
        mob.layer = LayerMask.NameToLayer("Mobs");
        mob.name = prefab.name;

        // Start slightly above the ground
        mob.transform.position = pos + new Vector3(0f, 5f, 0f);

        return mob;
    }
}
