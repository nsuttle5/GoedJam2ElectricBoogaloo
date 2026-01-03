#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public static class LogFilter
{


	private static readonly string[] SuppressIfContains =
	{
		"can't calculate tangents",
		"cant calculate tangents",
		"Cannot calculate tangents",
		"Can't calculate tangents",
	};



	static LogFilter()
	{
		Application.logMessageReceived -= OnLog;
		Application.logMessageReceived += OnLog;
	}



	private static void OnLog(string condition, string stackTrace, LogType type)
	{
		if (type != LogType.Warning) return;


		if (string.IsNullOrEmpty(condition)) return;

		for (int i = 0; i < SuppressIfContains.Length; i++)
		{
			if (condition.IndexOf(SuppressIfContains[i], StringComparison.OrdinalIgnoreCase) >= 0)
			{
				//
				return;
			}
		}
	}


}
#endif
