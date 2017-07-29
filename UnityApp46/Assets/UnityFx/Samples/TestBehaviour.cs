﻿using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityFx.Async;

public class TestBehaviour : MonoBehaviour
{
	private AsyncFactory _opFactory;

	private IEnumerator Start()
	{
		_opFactory = new AsyncFactory(this);

		yield return StartCoroutine(AsyncFactoryTest());
		yield return StartCoroutine(ContinuationTest());

		AwaitTest();
	}

	private IEnumerator AsyncFactoryTest()
	{
		int n = 0;

		var op1 = _opFactory.FromUpdateCallback(c =>
		{
			if (++n < 100)
			{
				c.SetProgress(n / 100f);
			}
			else
			{
				c.SetCompleted();
			}
		});

		var op2 = _opFactory.FromEnumerator(WaitSecondEnum());
		var op3 = _opFactory.FromCoroutine(new WaitForSeconds(2));
		var op4 = Task.Run(() => Thread.Sleep(1000));

		var op5 = _opFactory.FromUpdateCallback(c =>
		{
			throw new Exception("test exception");
		});

		yield return _opFactory.WhenAll(op1, op2, op3, op4, op5);

		Debug.Log(op1);
		Debug.Log(op2);
		Debug.Log(op3);
		Debug.Log(op4);
		Debug.Log(op5);
	}

	private IEnumerator ContinuationTest()
	{
		var op1 = _opFactory.FromEnumerator(WaitSecondEnum());
		var op2 = op1.ContinueWith(op => _opFactory.FromCoroutine(new WaitForSeconds(1)));

		yield return op2;

		Debug.Log(op1);
		Debug.Log(op2);
	}

	private async void AwaitTest()
	{
		var op = _opFactory.FromCoroutine(new WaitForSeconds(1));
		await op;

		if (op.IsCompletedSuccessfully)
		{
			Debug.Log("op has finished");
		}
		else
		{
			Debug.Log("op has failed");
		}

	}

	private IEnumerator WaitSecondEnum()
	{
		yield return new WaitForSeconds(1);
	}
}
