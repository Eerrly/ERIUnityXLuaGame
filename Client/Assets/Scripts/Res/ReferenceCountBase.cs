public class ReferenceCountBase
{
    private int _referenceCount;

    /// <summary>
    /// 引用计数
    /// </summary>
    public int ReferenceCount => _referenceCount;

    public bool IsReferenceValid => _referenceCount > 0;

    public ReferenceCountBase()
    {
        _referenceCount = 1;
    }

    /// <summary>
    /// 引用计数+1
    /// </summary>
    public virtual void Retain() { ++_referenceCount; }

    /// <summary>
    /// 引用计数-1
    /// </summary>
    public virtual void Release() { if (--_referenceCount == 0) OnReferenceBecameInvalid(); }

    /// <summary>
    /// 引用计数为0时触发
    /// </summary>
    public virtual void OnReferenceBecameInvalid() { }

}
