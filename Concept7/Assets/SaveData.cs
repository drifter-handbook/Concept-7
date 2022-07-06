using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    public static SaveData Instance;

    public int EnemiesKilled;
    public int FurthestCompletedLevel;

    public void Load()
    {
        EnemiesKilled = GetKey("EnemiesKilled", 0);
        FurthestCompletedLevel = GetKey("FurthestCompletedLevel", 0);
    }
    public void Save()
    {
        SetKey("EnemiesKilled", EnemiesKilled);
        SetKey("FurthestCompletedLevel", FurthestCompletedLevel);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        Load();
    }

    void SetKey<T>(string key, T val)
    {
        if (typeof(T) == typeof(int))
        {
            PlayerPrefs.SetInt(key, (int)(object)val);
            return;
        }
        if (typeof(T) == typeof(float))
        {
            PlayerPrefs.SetFloat(key, (float)(object)val);
            return;
        }
        if (typeof(T) == typeof(string))
        {
            PlayerPrefs.SetString(key, (string)(object)val);
            return;
        }
        throw new InvalidOperationException($"SaveData.SetKey type argument {typeof(T).Name} must be int, float, or string");
    }

    T GetKey<T>(string key, T def=default)
    {
        if (typeof(T) == typeof(int))
        {
            return (T)(object)PlayerPrefs.GetInt(key, (int)(object)def);
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)def);
        }
        if (typeof(T) == typeof(string))
        {
            return (T)(object)PlayerPrefs.GetString(key, (string)(object)def);
        }
        throw new InvalidOperationException($"SaveData.GetKey type argument {typeof(T).Name} must be int, float, or string");
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
