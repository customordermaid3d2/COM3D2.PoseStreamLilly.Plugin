using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.PoseStreamLilly.Plugin
{
	class BoneDataA
	{
		public String name = "";
		public bool isB = false;
		public List<BoneDataB> b = null;
		public byte[] outputABinary()
		{
			List<byte> lb = new List<byte>();
			//01
			lb.Add(1);
			//(ボーン名の長さ)
			//(본 이름의 길이)
			lb.Add((byte)(name.Length));
			//[01](長いボーン名だとある？)
			//[01](긴 뼈 이름이라고있다?)
			if (isB)
			{
				lb.Add(1);
			}
			//ボーン名
			char[] c = name.ToCharArray();
			for (int i = 0; i < name.Length; i++)
			{
				lb.Add((byte)(c[i]));
			}
			//B
			foreach (BoneDataB b1 in b)
			{
				lb.AddRange(b1.outputBBinary());
			}
			return lb.ToArray();
		}

	}

	class BoneDataB
	{
		public int index = 0;
		public List<BoneDataC> c = null;
		public byte[] outputBBinary()
		{
			List<byte> lb = new List<byte>();
			//XX(64から始まる)
			//XX(64에서 시작)
			lb.Add((byte)index);
			//XX XX XX XX(フレーム数)
			//XX XX XX XX(프레임 수)
			lb.Add((byte)((c.Count) % 256));
			lb.Add((byte)(((c.Count) / 256) % 256));
			lb.Add((byte)((((c.Count) / 256) / 256) % 256));
			lb.Add((byte)(((((c.Count) / 256) / 256) / 256) % 256));
			//C
			foreach (BoneDataC c1 in c)
			{
				lb.AddRange(c1.outputCBinary());
			}
			return lb.ToArray();
		}
	}

	/// <summary>
	/// 중간값용 데이터를위해 개조
	/// </summary>
	class BoneDataC
	{
		public int time = 0;
		public int time2 = 0;
		public byte[] raw = null; //12바이트 실제 포즈값인듯
		public byte[] raw2 = null;
		public byte[] outputCBinary()
		{
			List<byte> lb = new List<byte>();
			float f = time / 1000f;
			byte[] b = new byte[4];
			byte[] t = BitConverter.GetBytes(f);
			b[0] = t[0];
			b[1] = t[1];
			b[2] = t[2];
			b[3] = t[3];
			//時間
			//시간
			lb.AddRange(b);
			//データ
			//데이터
			lb.AddRange(raw);
			//Debug.Log("BoneDataC lb " + lb.ToString());
			return lb.ToArray();
		}


		/// <summary>
		/// 실제 중간값 얻는용
		/// </summary>
		public void rawMid()
		{
			//Debug.Log("raw " + BitConverter.ToSingle(raw, 0) + " / " + BitConverter.ToSingle(raw2, 0));
			raw = (BitConverter.GetBytes((float)(BitConverter.ToSingle(raw, 0) + BitConverter.ToSingle(raw2, 0)) / 2));
			System.Array.Resize(ref raw, 12);
			//Debug.Log("raw " + BitConverter.ToSingle(raw, 0) );
		}


	}
}
