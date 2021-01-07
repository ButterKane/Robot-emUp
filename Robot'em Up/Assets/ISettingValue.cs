using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettingValue
{
	void Init (Setting setting);
	void IncreaseValue ();
	void DecreaseValue ();

	void ResetValue ();
	float GetValue ();
}
