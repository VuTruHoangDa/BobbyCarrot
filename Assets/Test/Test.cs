using UnityEngine;


public class Test : MonoBehaviour
{
	private void Start()
	{
		X x = new B();

		print(x is A or B);
	}
}


class X { }

class A : X { }

class B :X { }