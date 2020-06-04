using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiodeManager : MonoBehaviour
{
	public static Dictionary<int, List<DiodeController>> diodeDictionnary = new Dictionary<int, List<DiodeController>>();
	public static Dictionary<int, List<NeonDiode>> neonDictionnary = new Dictionary<int, List<NeonDiode>>();

	public static void RegisterDiodeController(DiodeController dc, int id)
	{
		List<DiodeController> registeredControllers = new List<DiodeController>();
		if (diodeDictionnary.ContainsKey(id)) {
			registeredControllers = diodeDictionnary[id];
		}
		registeredControllers.Add(dc);
		diodeDictionnary[id] = registeredControllers;
	}

	public static void ActivateDiodes ( int id )
	{
		List<DiodeController> registeredControllers = diodeDictionnary[id];
		if (registeredControllers != null)
		{
			foreach (DiodeController dc in registeredControllers)
			{
				dc.Activate();
			}
		}
	}

	public static void ActivateNeons( int id)
	{
		List<NeonDiode> registeredControllers = neonDictionnary[id];
		if (registeredControllers != null)
		{
			foreach (NeonDiode dc in registeredControllers)
			{
				dc.Activate();
			}
		}
	}

	public static void RegisterNeonController(NeonDiode nc, int id)
	{
		List<NeonDiode> registeredControllers = new List<NeonDiode>();
		if (neonDictionnary.ContainsKey(id))
		{
			registeredControllers = neonDictionnary[id];
		}
		registeredControllers.Add(nc);
		neonDictionnary[id] = registeredControllers;
	}
}
