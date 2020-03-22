using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class IntStringDictionary : SerializableDictionary<int, string> { }

[Serializable]
public class IntSceneDictionary : SerializableDictionary<int, SceneAsset> { }

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}
#endif