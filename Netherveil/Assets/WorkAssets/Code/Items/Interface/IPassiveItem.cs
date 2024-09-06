using System.Collections;
using UnityEngine;

public interface IPassiveItem : IItem{
    public sealed IEnumerator DisableUntil(System.Func<bool> predicate)
    {
        OnRemove();
        yield return new WaitUntil(predicate);
        OnRetrieved();
    }
}
