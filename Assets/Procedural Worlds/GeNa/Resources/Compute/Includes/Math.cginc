#ifndef GENA_MATH
#define GENA_MATH

inline float3 normalize_safe(float3 v, float3 fallback)
{
    float vv = dot(v, v);
    return vv > 1e-16f ? v / sqrt(vv) : fallback;
}

inline float3 project_vec(float3 v, float3 onto)
{
    onto = normalize(onto);
    return dot(v, onto) * onto;
}

inline float3 project_plane(float3 v, float3 n)
{
    return v - project_vec(v, n);
}

inline float3 limit_length(float3 v, float maxLen)
{
    return min(maxLen, length(v)) * normalize_safe(v, 0.0f);
}

inline float angle(float3 a, float3 b)
{
    float d = clamp(dot(normalize(a), normalize(b)), -1.0, 1.0);
    return acos(d) * 57.295779513;
}

inline float4 quat_conj(float4 q)
{
    return float4(-q.xyz, q.w);
}

inline float4 quat_inv(float4 q)
{
    return quat_conj(q);
}

inline float3 quat_rot(float4 q, float3 v)
{
    return
        dot(q.xyz, v) * q.xyz
        + q.w * q.w * v
        + 2.0 * q.w * cross(q.xyz, v)
        - cross(cross(q.xyz, v), q.xyz);
}

inline float max_comp(float3 v)
{
    return max(v.x, max(v.y, v.z));
}

float sdf_sphere(float3 p, float radius, float3 c, float r)
{
    p -= c;
    float totalRadius = radius + r;
    return length(p) - totalRadius;
}

float sdf_uni_cubic(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0f) / k;
    return min(a, b) - h * h * h * k * (1.0f / 6.0f);
}

inline float sdf_uni_smooth(float a, float b, float h)
{
    return sdf_uni_cubic(a, b, h);
}

float sdf_sub_cubic(float a, float b, float k)
{
    float h = max(k - abs(a + b), 0.0f) / k;
    return max(a, -b) + h * h * h * k * (1.0f / 6.0f);
}

inline float sdf_sub_smooth(float a, float b, float h)
{
    return sdf_sub_cubic(a, b, h);
}

float sdf_int_cubic(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0f) / k;
    return max(a, b) + h * h * h * k * (1.0f / 6.0f);
}

inline float sdf_int_smooth(float a, float b, float h)
{
    return sdf_int_cubic(a, b, h);
}

float sdf_box(float3 p, float radius, float3 c, float3 h, float4 q = float4(0.0f, 0.0f, 0.0f, 1.0f), float r = 0.0f)
{
    p = quat_rot(quat_inv(q), p - c);
    float3 d = abs(p) - h;
    float totalRadius = radius + r;
    return length(max(d, 0.0f)) + min(max_comp(d), 0.0f) - totalRadius;
}

float sdf_capsule(float3 p, float radius, float3 a, float3 b, float r)
{
    float3 ab = b - a;
    float3 ap = p - a;
    p -= a + saturate(dot(ap, ab) / dot(ab, ab)) * ab;
    float totalRadius = radius + r;
    return length(p) - totalRadius;
}

float sdf_cylinder(float3 p, float radius, float3 a, float3 b, float r)
{
    float result = 0.0f;
    float3 ab = b - a;
    float3 ap = p - a;
    float t = dot(ap, ab) / dot(ab, ab);
    float3 q = a + saturate(t) * ab;
    float totalRadius = radius + r;
    if (t >= 0.0f && t <= 1.0f)
        result = length(p - q) - totalRadius;
    else
    {
        float3 c = q + limit_length(project_plane(p - q, ab), totalRadius);
        result = length(p - c);
    }
    return result;
}

struct SdfShape
{
    int layer;
    int4 data0; // type, operator
    // sphere     box        capsule    cylinder
    float4 data1; // c.xyz, r   c.xyz, r   a.xyz, r   a.xyz, r
    float4 data2; // h.xyz      b.xyz      b.xyz
    float4 data3; // q
};

struct AabbTest
{
    float3 startPosition;
    float3 position;
    float3 normal;
    float hit;
    int message;
};

float sdf_shape(float3 p, float radius, SdfShape s)
{
    float result = 1e32f;
    // 0 = Sphere, 1 = Box, 2 = Capsule, 3 = Cylinder
    if (s.data0.x == 0)
        result = sdf_sphere(p, radius, s.data1.xyz, s.data1.w);
    else if (s.data0.x == 1)
        result = sdf_box(p, radius, s.data1.xyz, s.data2.xyz, s.data3, s.data1.w);
    else if (s.data0.x == 2) 
        result = sdf_capsule(p, radius, s.data1.xyz, s.data2.xyz, s.data1.w);
    else if (s.data0.x == 3)
        result = sdf_cylinder(p, radius, s.data1.xyz, s.data2.xyz, s.data1.w);
    return result;
}

// 0 = Union, 1 = Subtraction, 2 = Intersection
#define SDF_NEAR_SHAPES(res, p, radius, aiNearShape, numNearShapes, layer)                      \
  {                                                                                             \
    float3 opRes = 1e32f;                                                                       \
    for (int i = 0; i < numNearShapes; ++i)                                                     \
    {                                                                                           \
      const int iShape = aiNearShape[i];                                                        \
      const int op = aSdfShape[iShape].data0.y;                                                 \
      if ((layer & aSdfShape[iShape].layer) <= 0)                                               \
      {                                                                                         \
        continue;                                                                               \
      }                                                                                         \
      if (op == 0)                                                                              \
      {                                                                                         \
        opRes.x = sdf_uni_smooth(opRes.x, sdf_shape(p, radius, aSdfShape[iShape]), blendDist);  \
      }                                                                                         \
      else if (op == 1)                                                                         \
      {                                                                                         \
        opRes.y = sdf_uni_smooth(opRes.y, sdf_shape(p, radius, aSdfShape[iShape]), blendDist);  \
      }                                                                                         \
      else if (op == 2)                                                                         \
      {                                                                                         \
        opRes.z = sdf_uni_smooth(opRes.z, sdf_shape(p, radius, aSdfShape[iShape]), blendDist);  \
      }                                                                                         \
    }                                                                                           \
    res = sdf_sub_smooth(opRes.x, opRes.y, blendDist);                                          \
    if (opRes.z < 1e32f)                                                                        \
      res = sdf_int_smooth(res, opRes.z, blendDist);                                            \
  }


struct Aabb
{
    float4 boundsMin;
    float4 boundsMax;
};

struct AabbNode
{
    Aabb aabb;
    int nextFree;
    int parent;
    int childA;
    int childB;
    int height;
    int shapeIndex;
};

inline Aabb aabb_expand(Aabb aabb, float radius)
{
    aabb.boundsMin.x -= radius;
    aabb.boundsMin.y -= radius;
    aabb.boundsMin.z -= radius;
    aabb.boundsMax.x += radius;
    aabb.boundsMax.y += radius;
    aabb.boundsMax.z += radius;
    return aabb;
}

inline bool aabb_intersects(Aabb a, Aabb b)
{
    return all(a.boundsMin <= b.boundsMax && a.boundsMax >= b.boundsMin);
}

// stmt = statements processing shapeIndex of hit leaf AABB nodes
#define aabb_tree_query(tree, root, rayBounds, extents, stackSize, stmt)      \
{                                                                             \
  int stackTop = 0;                                                           \
  int stack[stackSize];                                                       \
  stack[stackTop] = root;                                                     \
                                                                              \
  while (stackTop >= 0)                                                       \
  {                                                                           \
    int index = stack[stackTop--];                                            \
    if (index < 0)                                                            \
      continue;                                                               \
                                                                              \
    Aabb tightBounds = tree[index].aabb;                                      \
    tightBounds = aabb_expand(tightBounds, extents);                          \
    if (!aabb_intersects(rayBounds, tightBounds))                             \
      continue;                                                               \
    if (tree[index].childA < 0)                                               \
    {                                                                         \
      const int shapeIndex = tree[index].shapeIndex;                          \
                                                                              \
      stmt                                                                    \
    }                                                                         \
    else                                                                      \
    {                                                                         \
      stackTop = min(stackTop + 1, stackSize - 1);                            \
      stack[stackTop] = tree[index].childA;                                   \
      stackTop = min(stackTop + 1, stackSize - 1);                            \
      stack[stackTop] = tree[index].childB;                                   \
    }                                                                         \
  }                                                                           \
}

#endif
