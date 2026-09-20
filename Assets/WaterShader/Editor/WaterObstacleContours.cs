using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meganeura.Water.Editor
{
    // Editor-only scalar contour interpolation, not a fluid/time/pressure solver.
    // The centerline's signed transverse coordinate labels streamlines. A solid gets
    // one label; smoothly interpolating its displacement bends contours around it.
    // A screened Laplacian limits the correction's reach and preserves distant data.
    internal static class WaterObstacleContours
    {
        public static Vector2 Sample(Vector2[] values, int size, Vector2 uv)
        {
            float x=Mathf.Clamp(uv.x*size-.5f,0,size-1),y=Mathf.Clamp(uv.y*size-.5f,0,size-1);
            int ix=Mathf.Min((int)x,size-2),iy=Mathf.Min((int)y,size-2);
            return Vector2.Lerp(Vector2.Lerp(values[iy*size+ix],values[iy*size+ix+1],x-ix),
                Vector2.Lerp(values[(iy+1)*size+ix],values[(iy+1)*size+ix+1],x-ix),y-iy);
        }

        public static float Sample(float[] values, int size, Vector2 uv)
        {
            float x = Mathf.Clamp(uv.x * size - .5f, 0, size-1), y = Mathf.Clamp(uv.y * size - .5f, 0, size-1);
            int ix = Mathf.Min((int)x,size-2), iy = Mathf.Min((int)y,size-2);
            return Mathf.Lerp(Mathf.Lerp(values[iy*size+ix],values[iy*size+ix+1],x-ix),
                Mathf.Lerp(values[(iy+1)*size+ix],values[(iy+1)*size+ix+1],x-ix),y-iy);
        }

        public static void RefineMesh(Mesh mesh, StylizedWaterFlowBaker baker, Collider[] obstacles, int size)
        {
            if (mesh.subMeshCount != 1)
                throw new InvalidOperationException("Obstacle-compatible chart generation currently requires a single-submesh water surface.");
            float coarse = Mathf.Max(baker.BakeSize.x, baker.BakeSize.y) / 100f;
            float fine = Mathf.Max(.025f, Mathf.Min(baker.BakeSize.x, baker.BakeSize.y) / size);
            // Shared edge decisions and 1/2/3-edge splits keep the adaptive mesh conforming.
            // Only the generated copy is refined; a rebake reuses sufficient density.
            for (int pass = 0; pass < 10; pass++)
            {
                var vertices = new List<Vector3>(mesh.vertices);
                int[] triangles = mesh.triangles;
                var edges = new Dictionary<ulong,int>();
                var refined = new List<int>(triangles.Length);
                bool changed = false;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int[] v = { triangles[i], triangles[i+1], triangles[i+2] };
                    int[] m = new int[3]; int splits = 0;
                    for (int e = 0; e < 3; e++)
                    {
                        int a=v[e], b=v[(e+1)%3];
                        ulong key=((ulong)(uint)Mathf.Min(a,b)<<32)|(uint)Mathf.Max(a,b);
                        if (!edges.TryGetValue(key, out m[e]))
                        {
                            Vector3 middle=(vertices[a]+vertices[b])*.5f;
                            float spacing=coarse;
                            foreach (Collider obstacle in obstacles)
                            {
                                Vector3 delta=baker.transform.InverseTransformVector(obstacle.ClosestPoint(baker.transform.TransformPoint(middle))-baker.transform.TransformPoint(middle));
                                if (new Vector2(delta.x,delta.z).magnitude < Mathf.Max(1f,baker.SteeringDistance*1.5f)) spacing=fine;
                            }
                            m[e]=-1;
                            if ((vertices[a]-vertices[b]).sqrMagnitude > spacing*spacing)
                            { m[e]=vertices.Count; vertices.Add(middle); }
                            edges.Add(key,m[e]);
                        }
                        if(m[e]>=0)splits++;
                    }
                    if(splits==0) {refined.AddRange(v);continue;}
                    changed=true;
                    if(splits==3)
                        refined.AddRange(new[]{v[0],m[0],m[2],m[0],v[1],m[1],m[2],m[1],v[2],m[0],m[1],m[2]});
                    else
                    {
                        int k=0;
                        while (!(m[k]>=0 && (splits==1 || m[(k+1)%3]>=0))) k++;
                        int a=v[k],b=v[(k+1)%3],c=v[(k+2)%3],ab=m[k];
                        if(splits==1) refined.AddRange(new[]{a,ab,c,ab,b,c});
                        else {int bc=m[(k+1)%3];refined.AddRange(new[]{a,ab,c,ab,bc,c,ab,b,bc});}
                    }
                }
                if(!changed)break;
                if(vertices.Count>200000)
                    throw new InvalidOperationException("Obstacle chart refinement exceeded 200,000 vertices. Use a smaller bake region or simpler water mesh.");
                mesh.Clear();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(vertices);mesh.SetTriangles(refined,0);
                mesh.RecalculateNormals();mesh.RecalculateBounds();
            }
        }

        public static Vector2[] Steer(StylizedWaterFlowBaker baker, bool[] water, bool[] blocked,
            float[] transverse, Vector2[] backbone, Vector2[] banks, int size, out float[] contourCoordinates)
        {
            contourCoordinates = null;
            int count = water.Length;
            var labels = new int[count];
            var correction = new float[count];
            var queue = new Queue<int>();
            float dx = baker.BakeSize.x / size, dz = baker.BakeSize.y / size;
            float largestRadius = 0f;
            int component = 0;
            for (int seed = 0; seed < count; seed++)
            {
                if (!blocked[seed] || labels[seed] != 0) continue;
                component++;
                var cells = new List<int>();
                queue.Enqueue(seed); labels[seed] = component;
                float sum = 0, bankSum = 0, bankMin = float.MaxValue, bankMax = float.MinValue;
                int bankCount = 0;
                while (queue.Count > 0)
                {
                    int i = queue.Dequeue(); cells.Add(i); sum += transverse[i];
                    int x = i % size, y = i / size;
                    for (int oy = -1; oy <= 1; oy++) for (int ox = -1; ox <= 1; ox++)
                    {
                        if (ox == 0 && oy == 0) continue;
                        int nx = x + ox, ny = y + oy;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) continue;
                        int j = ny * size + nx;
                        if (blocked[j] && labels[j] == 0) { labels[j] = component; queue.Enqueue(j); }
                        if (!water[j])
                        {
                            bankSum += transverse[j]; bankCount++;
                            bankMin = Mathf.Min(bankMin, transverse[j]); bankMax = Mathf.Max(bankMax, transverse[j]);
                        }
                    }
                }
                // A bank-attached intrusion shares the bank contour. Crossing both
                // sides cannot be solved by a local steering field.
                if (bankCount > 0 && bankMin < -2 * dz && bankMax > 2 * dz)
                    throw new InvalidOperationException("[Water Flow Baker] Obstacle spans both sides of the channel. Clear a passage or edit the centerline; no automatic rerouting was attempted.");
                float contour = bankCount > 0 ? bankSum / bankCount : sum / cells.Count;
                foreach (int i in cells) correction[i] = contour - transverse[i];
                largestRadius = Mathf.Max(largestRadius, Mathf.Sqrt(cells.Count * dx * dz / Mathf.PI));
            }
            if (component == 0) return banks;

            // Strength controls reach, not solid permeability. Distance is in local
            // XZ units, not square UV texels (which distorted the old gradient).
            float reach = Mathf.Max(2 * largestRadius, baker.SteeringDistance * (.5f + baker.SteeringStrength));
            float wx = 1f / (dx * dx), wz = 1f / (dz * dz);
            float denominator = 2 * (wx + wz) + 1f / (reach * reach);
            var free = new List<int>();
            for (int y = 1; y < size - 1; y++) for (int x = 1; x < size - 1; x++)
            {
                int i = y * size + x;
                if (water[i] && !blocked[i]) free.Add(i);
            }
            // SOR converges a static interpolation; stopping tolerance scales with
            // cell size so the derivative remains stable across bake resolutions.
            float tolerance = Mathf.Min(dx, dz) * 0.00002f;
            float residual = 0;
            int iterations = 0;
            for (; iterations < size * 8; iterations++)
            {
                residual = 0;
                foreach (int i in free)
                {
                    float target = (wx * (correction[i-1] + correction[i+1]) + wz * (correction[i-size] + correction[i+size])) / denominator;
                    float change = target - correction[i];
                    correction[i] += 1.8f * change;
                    residual = Mathf.Max(residual, Mathf.Abs(change));
                }
                if (residual < tolerance) break;
            }
            if (residual >= tolerance)
                throw new InvalidOperationException("[Water Flow Baker] Obstacle contour interpolation did not converge. Reduce bake resolution or simplify the obstruction.");

            var result = (Vector2[])banks.Clone();
            contourCoordinates = new float[count];
            for (int i = 0; i < count; i++) contourCoordinates[i] = transverse[i] + correction[i];
            int reverse = 0;
            foreach (int i in free)
            {
                // Sobel derivative suppresses occupancy stair steps without blurring
                // directions across the solid or changing Flow Strength.
                float gx = (correction[i+1-size] + 2*correction[i+1] + correction[i+1+size]
                    - correction[i-1-size] - 2*correction[i-1] - correction[i-1+size]) / (8*dx);
                float gz = (correction[i+size-1] + 2*correction[i+size] + correction[i+size+1]
                    - correction[i-size-1] - 2*correction[i-size] - correction[i-size+1]) / (8*dz);
                Vector2 localBase = Vector2.Scale(backbone[i], baker.BakeSize).normalized;
                Vector2 localBanks = Vector2.Scale(banks[i], baker.BakeSize).normalized;
                Vector2 direction = localBase + new Vector2(gz, -gx);
                // Do not add bank repulsion near a solid: both constraints are already
                // present in the scalar interpolation. Preserve bank-only behavior far away.
                float influence = Mathf.Clamp01(new Vector2(gx,gz).magnitude * 12f);
                direction += (localBanks-localBase) * (1-influence);
                if (Vector2.Dot(direction, localBase) <= 0) reverse++;
                result[i] = new Vector2(direction.x / baker.BakeSize.x, direction.y / baker.BakeSize.y).normalized;
            }
            if (reverse > 0)
                Debug.LogWarning($"[Water Flow Baker] {reverse} obstacle samples lack downstream progression. Inspect Direction Debug; this obstruction may exceed local steering scope.", baker);

            // Bilinear sampling at the silhouette must not blend back to the original
            // centerline INSIDE the solid. Extend the nearest exterior vector inward.
            var filled = new bool[count];
            queue.Clear();
            for (int i = 0; i < count; i++) if (water[i] && !blocked[i]) { filled[i] = true; queue.Enqueue(i); }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue(), x = i % size, y = i / size;
                for (int side = 0; side < 4; side++)
                {
                    int nx = x + (side == 0 ? -1 : side == 1 ? 1 : 0);
                    int ny = y + (side == 2 ? -1 : side == 3 ? 1 : 0);
                    if (nx < 0 || nx >= size || ny < 0 || ny >= size) continue;
                    int j = ny*size+nx;
                    if (!blocked[j] || filled[j]) continue;
                    result[j] = result[i]; filled[j] = true; queue.Enqueue(j);
                }
            }
            Debug.Log($"[Water Flow Baker] Obstacle contours: {component} components, {iterations+1} interpolation passes, residual {residual:G3}.", baker);
            return result;
        }
    }
}
