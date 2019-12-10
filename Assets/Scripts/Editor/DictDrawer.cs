using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IntNodeDictionary))]
public class DictDrawer : SerializableDictionaryPropertyDrawer {}

