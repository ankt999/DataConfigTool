using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;


namespace Table
{
    [Serializable]
    public class weapon
    {
      /// <summary>
      /// 唯一ID
      /// </summary>
      public int id;
      /// <summary>
      /// 武器名称
      /// </summary>
      public string name;
      /// <summary>
      /// 攻击力
      /// </summary>
      public float atk;
      /// <summary>
      /// 是否绑定
      /// </summary>
      public bool isBind;
    }
}
