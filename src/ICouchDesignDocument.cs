// Decompiled with JetBrains decompiler
// Type: Divan.ICouchDesignDocument
// Assembly: Divan, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 90D41068-8A8E-4101-8D09-BFE505435D2D
// Assembly location: D:\Workspaces\AG\Sicherung\Divan.dll

namespace Divan
{
  public interface ICouchDesignDocument : ICouchDocument, ICanJson
  {
    ICouchDatabase Owner { get; set; }
  }
}
