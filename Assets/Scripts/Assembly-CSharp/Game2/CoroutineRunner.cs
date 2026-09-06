using System.Collections;
using UnityEngine;

namespace Game2
{
	public class CoroutineRunner : MonoBehaviour
	{
		private static CoroutineRunner _instance;

		public static CoroutineRunner Instance
		{
			get
			{
				if (_instance == null)
				{
					GameObject obj = new GameObject("CoroutineRunner");
					_instance = obj.AddComponent<CoroutineRunner>();
					Object.DontDestroyOnLoad(obj);
				}
				return _instance;
			}
		}

		public void RunCoroutine(IEnumerator coroutine)
		{
			StartCoroutine(coroutine);
		}
	}
}
