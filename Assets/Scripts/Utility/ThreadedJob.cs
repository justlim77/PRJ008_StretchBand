/* Implementation written by Bunny83
 * http://answers.unity3d.com/questions/357033/unity3d-and-c-coroutines-vs-threading.html
 */

using UnityEngine;
using System.Collections;

public class ThreadedJob
{
    private bool _IsDone = false;
    private object _Handle = new object();
    private System.Threading.Thread _Thread = null;

    public bool IsDone
    {
        get
        {
            bool temp;
            lock (_Handle)
            {
                temp = _IsDone;
            }
            return temp;
        }
        set
        {
            lock (_Handle)
            {
                _IsDone = value;
            }
        }
    }

    public virtual void Start()
    {
        _Thread = new System.Threading.Thread(Run);
        _Thread.Start();
        //Debug.Log("Thread started.");
    }

    public virtual void Abort()
    {
        _Thread.Abort();
        Debug.Log("Thread aborted.");
    }

    protected virtual void ThreadedFunction() { }
    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return null;
        }
    }

    void Run()
    {
        ThreadedFunction();
        IsDone = true;
    }
}
