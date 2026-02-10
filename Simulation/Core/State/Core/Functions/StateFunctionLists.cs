using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using Quantum.Collections;

namespace HnSF.core.state.functions
{
    [Serializable] public abstract unsafe class StateFunctionAssetRefQList : HNSFStateFunction<QList<AssetRef>> { }
    [Serializable] public abstract unsafe class StateFunctionBoolQList : HNSFStateFunction<QList<bool>> { }
    [Serializable] public abstract unsafe class StateFunctionByteQList : HNSFStateFunction<QList<byte>> { }
    [Serializable] public abstract unsafe class StateFunctionEntityRefQList : HNSFStateFunction<QList<EntityRef>> { }
    [Serializable] public abstract unsafe class StateFunctionEntityRefList : HNSFStateFunction<List<EntityRef>> { }
    [Serializable] public abstract unsafe class StateFunctionFPQList : HNSFStateFunction<QList<FP>> { }
    [Serializable] public abstract unsafe class StateFunctionFPVector2QList : HNSFStateFunction<QList<FPVector2>> { }
    [Serializable] public abstract unsafe class StateFunctionFPVector3QList : HNSFStateFunction<QList<FPVector3>> { }
    [Serializable] public abstract unsafe class StateFunctionIntegerQList : HNSFStateFunction<QList<int>> { }
    [Serializable] public abstract unsafe class StateFunctionIntegerList : HNSFStateFunction<List<int>> { }
}