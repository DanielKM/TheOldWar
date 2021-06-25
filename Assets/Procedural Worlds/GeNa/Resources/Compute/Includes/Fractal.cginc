#ifndef GENA_FRACTAL
#define GENA_FRACTAL
#include "../Includes/NoiseGenerator.cginc"

struct Fractal
{
    int type;
    int octaves;
    float seed;
    float xOffset;
    float yOffset;
    float zOffset;
    float frequency;
    float persistence;
    float lacunarity;
};

//-----------------------------------------------------------------------------
// FRACTAL
//-----------------------------------------------------------------------------

float Fractal_GetValue_Perlin(Fractal fractal, float x, float z)
{
    float value = 0.0f;
    float signal = 0.0f;
    float persistence = 1.0f;
    float nx, nz;

    x += fractal.seed;
    z += fractal.seed;
    x += fractal.xOffset;
    z += fractal.zOffset;
    x *= fractal.frequency;
    z *= fractal.frequency;

    for (int octave = 0; octave < fractal.octaves; octave++)
    {
        nx = x;
        nz = z;
        signal = NoiseGenerator_Generate(nx, nz);
        value += signal * persistence;
        x *= fractal.lacunarity;
        z *= fractal.lacunarity;
        persistence *= persistence;
    }

    value += fractal.yOffset * 2.4f;

    return value;
}

float Fractal_GetValue_Billow(Fractal fractal, float x, float z)
{
    float value = 0.0f;
    float signal = 0.0f;
    float persistence = 1.0f;
    float nx, nz;
    x += fractal.seed;
    z += fractal.seed;
    x += fractal.xOffset;
    z += fractal.zOffset;
    x *= fractal.frequency;
    z *= fractal.frequency;
    for (int octave = 0; octave < fractal.octaves; octave++)
    {
        nx = x;
        nz = z;
        signal = NoiseGenerator_Generate(nx, nz);
        signal = 2.0f * abs(signal) - 1.0f;
        value += signal * persistence;
        x *= fractal.lacunarity;
        z *= fractal.lacunarity;
        persistence *= persistence;
    }
    value += fractal.yOffset * 2.4f;
    return value;
}

/// <summary>
/// Calculate a ridged multi fractal
/// </summary>
/// <param name="fractal"></param>
/// <param name="x">x location</param>
/// <param name="z">z location</param>
/// <returns></returns>
float Fractal_GetValue_RidgedMulti(Fractal fractal, float x, float z)
{
    float signal = 0.0f;
    float value = 0.0f;
    float weight = 1.0f;
    float offset = 1.0f;
    float gain = fractal.persistence;
    float nx, nz;
    x += fractal.seed;
    z += fractal.seed;
    x += fractal.xOffset;
    z += fractal.zOffset;
    x *= fractal.frequency;
    z *= fractal.frequency;

    float spectralWeights[20];
    float h = 1.0f;
    float frequency = 1.0f;
    for (int i = 0; i < 20; i++)
    {
        spectralWeights[i] = pow(frequency, -h);
        frequency *= fractal.lacunarity;
    }

    for (int octave = 0; octave < fractal.octaves; octave++)
    {
        nx = x;
        nz = z;
        //Get the coherent-noise value from input value and add it to final result
        signal = NoiseGenerator_Generate(nx, nz);
        //Make the ridges
        signal = abs(signal);
        signal = offset - signal;
        //Square signal to increase sharpness of ridges.
        signal *= signal;
        //The weighting from previous octave is applied to the signal.
        signal *= weight;
        //Weight successive contributions by previous signal.
        weight = signal * gain;
        if (weight > 1.0) weight = 1.0f;
        if (weight < 0.0f) weight = 0.0f;
        //Add the signal to output value.
        value += signal * spectralWeights[octave];
        //Next Octave
        x *= fractal.lacunarity;
        z *= fractal.lacunarity;
    }
    value = value * 1.25f - 1.0f;
    value += fractal.yOffset;
    return value;
}

float Fractal_GetValue(Fractal fractal, float x, float z)
{
    float value = 0.0f;
    if (fractal.type == 0)
        value = Fractal_GetValue_Perlin(fractal, x, z);
    if (fractal.type == 1)
        value = Fractal_GetValue_Billow(fractal, x, z);
    if (fractal.type == 2)
        value = Fractal_GetValue_RidgedMulti(fractal, x, z);
    return value;
}

float Fractal_GetNormalisedValue(Fractal fractal, float x, float z)
{
    return clamp((Fractal_GetValue(fractal, x, z) + 1.0f) / 2.0f, 0.0f, 1.0f);
}

//-----------------------------------------------------------------------------
// END - FRACTAL
//-----------------------------------------------------------------------------


#endif
