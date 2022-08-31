public class ReferenceCountBase
{
    private int _referenceCount;

    public int ReferenceCount => _referenceCount;

    public bool IsReferenceValid => _referenceCount > 0;

    public ReferenceCountBase()
    {
        _referenceCount = 1;
    }

    public virtual void Retain() { ++_referenceCount; }

    public virtual void Release() { if (--_referenceCount == 0) OnReferenceBecameInvalid(); }

    public virtual void OnReferenceBecameInvalid() { }

}
