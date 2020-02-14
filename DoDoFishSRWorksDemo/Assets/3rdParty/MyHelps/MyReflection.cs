using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MyReflection
{
    /// <summary>
    /// https://stackoverflow.com/questions/1340438/get-value-of-static-field?rq=1
    /// </summary>
    /// <param name="consoleType">the type try to get variable</param>
    /// <param name="propName">variable name</param>
    /// <returns></returns>
    public static object GetStaticMemberVariable(Type consoleType, string propName)
    {
        object final = null;
        //PropertyInfo info = consoleType.GetProperty(propName, BindingFlags.Public | BindingFlags.Static);
        FieldInfo f = consoleType.GetField(propName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
            final = f.GetValue(null);

        return final;
        //if (final == null)
        //{
        //    //Debug.LogError("[MyReflection][GetMemberVariable]GetField.GetValue fail : " + typeof(T));
        //}
        //else if (final is T)
        //{
        //    //Debug.LogWarning("[MyReflection][GetMemberVariable]cast success, type : " + final.GetType() + ", T :" + typeof(T));
        //    return (T)final;
        //}
        //else
        //{
        //    //Debug.LogError("[MyReflection][GetMemberVariable]cast fail, type : " + final.GetType() + ", T :" + typeof(T));
        //}
        //return null;
    }

    /// <summary>
    /// http://stackoverflow.com/questions/7649324/c-sharp-reflection-get-field-values-from-a-simple-class
    /// according name to get member object
    /// </summary>
    public static object GetMemberVariable(object src, string propName)
    {
        object final = null;
        Type t = src.GetType();
        //PropertyInfo pi = t.GetProperty(propName);
        //if (pi == null)
        //    Debug.LogError("[MyReflection] [GetProperty] : null : " + t.ToString() + " , propName : " + propName);
        //return pi.GetValue(src, null);

        FieldInfo fields = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (fields != null)
        {
            final = fields.GetValue(src);
            //Debug.Log("[MyReflection][GetMemberVariable] variable : " + fields.Name);
        }

        return final;
        //if (final == null)
        //{
        //    //Debug.LogError("[MyReflection][GetMemberVariable]GetField.GetValue fail : " + typeof(T));
        //}
        //else if (final is T)
        //{
        //    //Debug.LogWarning("[MyReflection][GetMemberVariable]cast success, type : " + final.GetType() + ", T :" + typeof(T));
        //    return final;
        //}
        //else
        //{
        //    //Debug.LogError("[MyReflection][GetMemberVariable]cast fail, type : " + final.GetType() + ", T :" + typeof(T));
        //}
        //return null;
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/system.reflection.fieldinfo.getvalue(v=vs.110).aspx
    /// 由傳入的class instance(src)找出我要的類型成員(T)
    /// </summary>
    public static T[] GetMemberVariable<T>(object src, out string[] memberNames)
    {
        List<string> memberNameList = new List<string>();

        List<T> outList = new List<T>();
        FieldInfo[] classFields = src.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo classField in classFields)
        {
            object field = classField.GetValue(src);
            if (field is T)
            {
                outList.Add((T)field);
                memberNameList.Add(classField.Name);
            }
        }
        memberNames = memberNameList.ToArray();
        return outList.ToArray();
    }

    public static object InvokeStaticMethod(string nameSpaceType, string methodName, object[] paramater)
    {
        try
        {
            // https://dotnetcodr.com/2014/10/15/dynamically-invoking-a-static-method-with-reflection-in-net-c/
            //string pathToDomain = Environment.CurrentDirectory+ "\\Unwrap.dll";
            //Assembly domainAssembly = Assembly.LoadFile(pathToDomain);
            //Type customerType = domainAssembly.GetType(nameSpaceType);
            //MethodInfo staticMethodInfo = customerType.GetMethod(methodName);
            //return staticMethodInfo.Invoke(null, paramater);


            Type consoleType = getType(nameSpaceType);
            // Type consoleType = Type.GetType("UnityEditor");

            Type[] methodParmsType = new Type[paramater.Length];
            for (int a = 0; a < paramater.Length; a++)
                methodParmsType[a] = paramater[a].GetType();

            MethodInfo magicMethod = consoleType.GetMethod(methodName, methodParmsType);
            return magicMethod.Invoke(null, paramater);
        }
        catch (Exception e)
        {
            Debug.LogError("[MyReflection][InvokeStaticMethod]" + nameSpaceType + " , " + methodName + " : " + e.Message);
            return null;
        }
    }

    static Type getType(string className)
    {
        //https://forum.unity.com/threads/using-type-gettype-with-unity-objects.136580/
        //The docs for System.Type.GetType say you need to use the assembly qualified name of the type.This means you need to include the name of the assembly after the full type name(including namespaces).So that would be:
        //Code(csharp):
        //Type.GetType("UnityEngine.GameObject, UnityEngine")
        //http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx
        //http://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx

        int start = className.LastIndexOf('.');
        //int end = className.Length - 1;
        string spaceName = "";
        if (start > 0)
            spaceName = "," + className.Substring(0, start);

        Type t = Type.GetType(className + "," + spaceName);
        if (t == null)
            Debug.LogError("[MyReflection][getType] : type not found : " + className + spaceName);
        return t;
    }
}
