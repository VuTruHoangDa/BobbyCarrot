using UnityEngine;


public class Test : MonoBehaviour
{
	public A a;
	private void Start()
	{
		a.i = 123;
		print(Instantiate(a).i);
	}
}