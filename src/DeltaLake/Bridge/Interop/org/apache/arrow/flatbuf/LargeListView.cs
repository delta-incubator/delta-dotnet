// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace org.apache.arrow.flatbuf
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

/// Same as ListView, but with 64-bit offsets and sizes, allowing to represent
/// extremely large data values.
public struct LargeListView : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static LargeListView GetRootAsLargeListView(ByteBuffer _bb) { return GetRootAsLargeListView(_bb, new LargeListView()); }
  public static LargeListView GetRootAsLargeListView(ByteBuffer _bb, LargeListView obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public LargeListView __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }


  public static void StartLargeListView(FlatBufferBuilder builder) { builder.StartTable(0); }
  public static Offset<org.apache.arrow.flatbuf.LargeListView> EndLargeListView(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<org.apache.arrow.flatbuf.LargeListView>(o);
  }
}


static public class LargeListViewVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
