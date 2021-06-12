using COM3D2.Lilly.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.PoseStreamLilly.Plugin
{
    class PoseStreamLillyUtill
    {

        internal static String anmName = "";

        private static String getPoseDataPath(bool b)
		{
			String path = Application.dataPath;
			path = path.Substring(0, path.Length - @"COM3D2x64_Data".Length);

			path += @"PhotoModeData\MyPose";
			if (b)
			{
				path += @"\";
			}
			return path;
		}

		internal static String anmMake(bool minAnmMake)
		{
			if (anmName.Length == 0)
			{
				return "이름이 입력되어 있지 않습니다";
			}
			String[] files = Directory.GetFiles(getPoseDataPath(false), anmName + @"_????????.anm", SearchOption.TopDirectoryOnly);
			if (files == null || files.Length == 0)
			{
				return "연결 대상의 포즈\n(" + anmName + "_00000000～)\n이 없습니다";
			}
			List<String> lf = new List<string>(files);
			List<String> ps = new List<string>();
			//Debug.Log("PoseStream3 ps.Count : " + ps.Count);

			foreach (String s in lf)
			{
				String s2 = s.Substring(getPoseDataPath(true).Length);
				if (s2.Length != (anmName.Length + @"_00000000.anm".Length))
				{
					//8桁でない
					//8 자리가 아니다
					continue;
				}
				if (get00000000byInt(s2) < 0)
				{
					//自然数でない
					//자연수가 아닌
					continue;
				}
				ps.Add(s2);
			}
			//Debug.Log("=== ps.toString ===");
			//Debug.Log(ps.ToString());
			//파일이 없을때
			if (ps.Count == 0)
			{
				return ("연결 대상의 포즈\n(" + anmName + "_00000000～)\n찾을 수 없습니다");
			}
			ps.Sort();
			//파일이 하나일때
			if (ps.Count == 1)
			{
				return ("연결 대상의 포즈\n(" + anmName + "_00000000～)\n이 하나 밖에 없습니다");
			}
			else if (get00000000byInt(ps[0]) != 0)
			{
				return ("첫 번째 포즈(" + anmName + "_00000000)이 없습니다");
			}
			Debug.Log("PoseStream3 ps.Count : " + ps.Count);
			if (minAnmMake)
			{
				// 중간 anm 생성
				if (makeAnmFileMid(ps.ToArray()))
				{
					Debug.Log("=== makeAnmFileMid ok ===");
					return "모션「" + anmName + ".anm」를 생성했습니다\n일단 다른 카테고리를 표시 후 마이뽀즈를 다시 표시하십시오";
				}
			}
			else
			{
				// 기존 anm생성
				if (makeAnmFile(ps.ToArray()))
				{
					Debug.Log("=== makeAnmFile ok ===");
					return "모션「" + anmName + ".anm」를 생성했습니다\n마이뽀즈 범주를 이미보고있는 경우\n일단 다른 카테고리를 표시 후 마이뽀즈를 다시 표시하십시오";
				}
			}


			return (errorFile);
		}

		public static String errorFile = "";

        private static bool makeAnmFile(String[] ss)
        {
            List<BoneDataA> bda = new List<BoneDataA>();
            byte[] header = new byte[15];
            bool isFirst = true;
            //読み込み
            //읽기
            foreach (String s in ss)
            {
                using (BinaryReader r = new BinaryReader(File.OpenRead(getPoseDataPath(true) + s)))
                {
                    try
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                            //最初のファイルでヘッダー部分を決定
                            //첫 번째 파일 헤더 부분을 결정
                            //14바이트
                            for (int i = 0; i < 15; i++)
                            {
                                header[i] = r.ReadByte();
                            }
                            byte t = r.ReadByte();
                            while (true)
                            {
                                if (t == 1)
                                {
                                    BoneDataA a = new BoneDataA();
                                    //A先頭部分
                                    //A 선두 부분
                                    t = r.ReadByte();
                                    byte c = 0;
                                    c = r.ReadByte();
                                    if (c == 1)
                                    {
                                        a.isB = true;
                                        a.name = "";
                                    }
                                    else
                                    {
                                        a.isB = false;
                                        a.name = "" + (char)c;
                                        t--;
                                    }
                                    for (int i = 0; i < t; i++)
                                    {
                                        c = r.ReadByte();
                                        a.name += (char)c;
                                    }
                                    a.b = new List<BoneDataB>();
                                    //B部分
                                    while (true)
                                    {
                                        t = r.ReadByte();
                                        if (t >= 64)
                                        {
                                            BoneDataB b = new BoneDataB();
                                            b.index = t;
                                            int tmpf = r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            //C部分
                                            bool firstFrame = true;
                                            for (int i = 0; i < tmpf; i++)
                                            {
                                                if (firstFrame)
                                                {
                                                    firstFrame = false;
                                                    b.c = new List<BoneDataC>();
                                                    BoneDataC bc = new BoneDataC();
                                                    bc.time = 0;
                                                    //time
                                                    r.ReadBytes(4);
                                                    //raw
                                                    bc.raw = r.ReadBytes(12);
                                                    b.c.Add(bc);
                                                }
                                                else
                                                {
                                                    r.ReadBytes(16);
                                                }
                                            }
                                            a.b.Add(b);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    bda.Add(a);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            int time = get00000000byInt(s);
                            //最初以外はヘッダー部分を読み飛ばす
                            //첫 이외는 헤더 부분을 건너
                            r.ReadBytes(15);
                            byte t = r.ReadByte();
                            while (true)
                            {
                                if (t == 1)
                                {
                                    //A先頭部分
                                    //A 선두 부분
                                    t = r.ReadByte();
                                    byte c = 0;
                                    String name;
                                    bool isB = false;
                                    c = r.ReadByte();
                                    if (c == 1)
                                    {
                                        isB = true;
                                        name = "";
                                    }
                                    else
                                    {
                                        isB = false;
                                        name = "" + (char)c;
                                        t--;
                                    }
                                    for (int i = 0; i < t; i++)
                                    {
                                        c = r.ReadByte();
                                        name += (char)c;
                                    }
                                    BoneDataA a = null;
                                    foreach (BoneDataA tmp in bda)
                                    {
                                        if (tmp.name.Equals(name))
                                        {
                                            a = tmp;
                                            break;
                                        }
                                    }
                                    if (a == null)
                                    {
                                        a = new BoneDataA();
                                        a.name = name;
                                        a.isB = isB;
                                        a.b = new List<BoneDataB>();
                                        //B部分
                                        //파트 B
                                        while (true)
                                        {
                                            t = r.ReadByte();
                                            if (t >= 64)
                                            {
                                                BoneDataB b = new BoneDataB();
                                                b.index = t;
                                                int tmpf = r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                //C部分
                                                bool firstFrame = true;
                                                for (int i = 0; i < tmpf; i++)
                                                {
                                                    if (firstFrame)
                                                    {
                                                        firstFrame = false;
                                                        b.c = new List<BoneDataC>();
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = 0;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                    else
                                                    {
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = time;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                }
                                                a.b.Add(b);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        bda.Add(a);
                                    }
                                    else
                                    {
                                        //B部分
                                        while (true)
                                        {
                                            t = r.ReadByte();
                                            if (t >= 64)
                                            {
                                                BoneDataB b = null;
                                                foreach (BoneDataB tmpb in a.b)
                                                {
                                                    if (t == tmpb.index)
                                                    {
                                                        b = tmpb;
                                                        break;
                                                    }
                                                }
                                                if (b == null)
                                                {
                                                    b = new BoneDataB();
                                                    b.index = t;
                                                    int tmpf = r.ReadByte();
                                                    r.ReadByte();
                                                    r.ReadByte();
                                                    r.ReadByte();
                                                    //C部分
                                                    bool firstFrame = true;
                                                    for (int i = 0; i < tmpf; i++)
                                                    {
                                                        if (firstFrame)
                                                        {
                                                            firstFrame = false;
                                                            b.c = new List<BoneDataC>();
                                                            BoneDataC bc = new BoneDataC();
                                                            bc.time = 0;
                                                            //time
                                                            r.ReadBytes(4);
                                                            //raw
                                                            bc.raw = r.ReadBytes(12);
                                                            b.c.Add(bc);
                                                        }
                                                        else
                                                        {
                                                            BoneDataC bc = new BoneDataC();
                                                            bc.time = time;
                                                            //time
                                                            r.ReadBytes(4);
                                                            //raw
                                                            bc.raw = r.ReadBytes(12);
                                                            b.c.Add(bc);
                                                        }
                                                    }
                                                    a.b.Add(b);
                                                }
                                                else
                                                {
                                                    int tmpf = r.ReadByte();
                                                    r.ReadByte();
                                                    r.ReadByte();
                                                    r.ReadByte();
                                                    //C部分
                                                    bool firstFrame = true;
                                                    for (int i = 0; i < tmpf; i++)
                                                    {
                                                        if (firstFrame)
                                                        {
                                                            firstFrame = false;
                                                            BoneDataC bc = new BoneDataC();
                                                            bc.time = time;
                                                            //time
                                                            r.ReadBytes(4);
                                                            //raw
                                                            bc.raw = r.ReadBytes(12);
                                                            b.c.Add(bc);
                                                        }
                                                        else
                                                        {
                                                            r.ReadBytes(16);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        errorFile = "ポーズ「" + s + "」\n로드하는 동안 오류가 발생했습니다";
                        return false;
                    }
                }
            }

            //結合
            //결합
            bool isExist = File.Exists(getPoseDataPath(true) + anmName + ".anm");
            using (BinaryWriter w = new BinaryWriter(File.Create(getPoseDataPath(true) + anmName + ".anm")))
            {
                try
                {
                    w.Write(header);
                    foreach (BoneDataA a in bda)
                    {
                        w.Write(a.outputABinary());
                    }
                    w.Write((byte)0);
                    w.Write((byte)0);
                    w.Write((byte)0);
                }
                catch (Exception)
                {
                    errorFile = "モーション「" + anmName + ".anm」\n내보내기 중에 오류가 발생했습니다";
                    return false;
                }
            }

            // 게임 모션 목록에 추가
            if (!isExist)
            {
                MotionWindow mw = GameObject.FindObjectOfType<MotionWindow>();
                if (mw != null)
                {
                    PopupAndTabList patl = mw.PopupAndTabList;
                    try
                    {
                        mw.AddMyPose(getPoseDataPath(true) + anmName + @".anm");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// 실제 파일 생성. 중간 생성
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        private static bool makeAnmFileMid(String[] ss)
        {

            //読み込み
            //읽기
            for (int f = 0; f < ss.Length - 1; f++)
            {
                // 본 데이터 목록 저장용

                String s = ss[f];
                String s2 = ss[f + 1];

                // 해더
                byte[] header = new byte[15];

                // 첫번째 포즈                
                List<BoneDataA> bda = new List<BoneDataA>();

                using (BinaryReader r = new BinaryReader(File.OpenRead(getPoseDataPath(true) + s)))
                {
                    try
                    {
                        int time = get00000000byInt(s);
                        //最初以外はヘッダー部分を読み飛ばす
                        //첫 이외는 헤더 부분을 건너
                        for (int i = 0; i < 15; i++)
                        {
                            header[i] = r.ReadByte();
                        }
                        byte t = r.ReadByte();
                        while (true)
                        {
                            if (t == 1)
                            {
                                //A先頭部分
                                //A 선두 부분
                                t = r.ReadByte();
                                byte c = 0;
                                String name;
                                bool isB = false;
                                c = r.ReadByte();
                                if (c == 1)
                                {
                                    isB = true;
                                    name = "";
                                }
                                else
                                {
                                    isB = false;
                                    name = "" + (char)c;
                                    t--;
                                }
                                for (int i = 0; i < t; i++)
                                {
                                    c = r.ReadByte();
                                    name += (char)c;
                                }
                                BoneDataA a = null;
                                foreach (BoneDataA tmp in bda)
                                {
                                    if (tmp.name.Equals(name))
                                    {
                                        a = tmp;
                                        break;
                                    }
                                }
                                if (a == null)
                                {
                                    a = new BoneDataA();
                                    a.name = name;
                                    a.isB = isB;
                                    a.b = new List<BoneDataB>();
                                    //B部分
                                    //파트 B
                                    while (true)
                                    {
                                        t = r.ReadByte();
                                        if (t >= 64)
                                        {
                                            BoneDataB b = new BoneDataB();
                                            b.index = t;
                                            int tmpf = r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            //C部分
                                            bool firstFrame = true;
                                            for (int i = 0; i < tmpf; i++)
                                            {
                                                if (firstFrame)
                                                {
                                                    firstFrame = false;
                                                    b.c = new List<BoneDataC>();
                                                    BoneDataC bc = new BoneDataC();
                                                    bc.time = 0;
                                                    //time
                                                    r.ReadBytes(4);
                                                    //raw
                                                    bc.raw = r.ReadBytes(12);
                                                    b.c.Add(bc);
                                                }
                                                else
                                                {
                                                    BoneDataC bc = new BoneDataC();
                                                    bc.time = time;
                                                    //time
                                                    r.ReadBytes(4);
                                                    //raw
                                                    bc.raw = r.ReadBytes(12);
                                                    b.c.Add(bc);
                                                }
                                            }
                                            a.b.Add(b);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    bda.Add(a);
                                }
                                else
                                {
                                    //B部分
                                    while (true)
                                    {
                                        t = r.ReadByte();
                                        if (t >= 64)
                                        {
                                            BoneDataB b = null;
                                            foreach (BoneDataB tmpb in a.b)
                                            {
                                                if (t == tmpb.index)
                                                {
                                                    b = tmpb;
                                                    break;
                                                }
                                            }
                                            if (b == null)
                                            {
                                                b = new BoneDataB();
                                                b.index = t;
                                                int tmpf = r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                //C部分
                                                bool firstFrame = true;
                                                for (int i = 0; i < tmpf; i++)
                                                {
                                                    if (firstFrame)
                                                    {
                                                        firstFrame = false;
                                                        b.c = new List<BoneDataC>();
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = 0;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                    else
                                                    {
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = time;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                }
                                                a.b.Add(b);
                                            }
                                            else
                                            {
                                                int tmpf = r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                //C部分
                                                bool firstFrame = true;
                                                for (int i = 0; i < tmpf; i++)
                                                {
                                                    if (firstFrame)
                                                    {
                                                        firstFrame = false;
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = time;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                    else
                                                    {
                                                        r.ReadBytes(16);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        errorFile = "ポーズ「" + s + "」로드하는 동안 오류가 발생했습니다";
                        return false;
                    }
                }

                using (BinaryReader r = new BinaryReader(File.OpenRead(getPoseDataPath(true) + s2)))
                {
                    try
                    {
                        int time = get00000000byInt(s);
                        //最初以外はヘッダー部分を読み飛ばす
                        //첫 이외는 헤더 부분을 건너
                        for (int i = 0; i < 15; i++)
                        {
                            header[i] = r.ReadByte();
                        }
                        byte t = r.ReadByte();
                        while (true)
                        {
                            if (t == 1)
                            {
                                //A先頭部分
                                //A 선두 부분
                                t = r.ReadByte();
                                byte c = 0;
                                String name;
                                bool isB = false;
                                c = r.ReadByte();
                                if (c == 1)
                                {
                                    isB = true;
                                    name = "";
                                }
                                else
                                {
                                    isB = false;
                                    name = "" + (char)c;
                                    t--;
                                }
                                for (int i = 0; i < t; i++)
                                {
                                    c = r.ReadByte();
                                    name += (char)c;
                                }
                                BoneDataA a = null;
                                foreach (BoneDataA tmp in bda)
                                {
                                    if (tmp.name.Equals(name))
                                    {
                                        a = tmp;
                                        break;
                                    }
                                }
                                if (a == null)
                                {
                                    a = new BoneDataA();
                                    a.name = name;
                                    a.isB = isB;
                                    a.b = new List<BoneDataB>();
                                    //B部分
                                    //파트 B
                                    while (true)
                                    {
                                        t = r.ReadByte();
                                        if (t >= 64)
                                        {
                                            BoneDataB b = new BoneDataB();
                                            b.index = t;
                                            int tmpf = r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            r.ReadByte();
                                            //C部分
                                            bool firstFrame = true;
                                            for (int i = 0; i < tmpf; i++)
                                            {
                                                if (firstFrame)
                                                {
                                                    firstFrame = false;
                                                    b.c = new List<BoneDataC>();
                                                    BoneDataC bc = new BoneDataC();
                                                    bc.time = 0;
                                                    //time
                                                    r.ReadBytes(4);
                                                    //raw
                                                    bc.raw = r.ReadBytes(12);
                                                    b.c.Add(bc);
                                                }
                                                else
                                                {
                                                    BoneDataC bc = new BoneDataC();
                                                    bc.time = time;
                                                    //time
                                                    r.ReadBytes(4);
                                                    //raw
                                                    bc.raw = r.ReadBytes(12);
                                                    b.c.Add(bc);
                                                }
                                            }
                                            a.b.Add(b);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    bda.Add(a);
                                }
                                else
                                {
                                    //B部分
                                    while (true)
                                    {
                                        t = r.ReadByte();
                                        if (t >= 64)
                                        {
                                            BoneDataB b = null;
                                            foreach (BoneDataB tmpb in a.b)
                                            {
                                                if (t == tmpb.index)
                                                {
                                                    b = tmpb;
                                                    break;
                                                }
                                            }
                                            if (b == null)
                                            {
                                                b = new BoneDataB();
                                                b.index = t;
                                                int tmpf = r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                //C部分
                                                bool firstFrame = true;
                                                for (int i = 0; i < tmpf; i++)
                                                {
                                                    if (firstFrame)
                                                    {
                                                        firstFrame = false;
                                                        b.c = new List<BoneDataC>();
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = 0;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                    else
                                                    {
                                                        BoneDataC bc = new BoneDataC();
                                                        bc.time = time;
                                                        //time
                                                        r.ReadBytes(4);
                                                        //raw
                                                        bc.raw = r.ReadBytes(12);
                                                        b.c.Add(bc);
                                                    }
                                                }
                                                a.b.Add(b);
                                            }
                                            else
                                            {
                                                int tmpf = r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                r.ReadByte();
                                                //C部分
                                                for (int i = 0; i < tmpf; i++)
                                                //for (int i = 0; i < 2; i++)
                                                {
                                                    //if (firstFrame)
                                                    //{
                                                    //    firstFrame = false;
                                                    //BoneDataC bc = new BoneDataC();
                                                    r.ReadBytes(4);
                                                    byte[] raw = r.ReadBytes(12);
                                                    BoneDataC bc = b.c[i];
                                                    bc.time = i;
                                                    //time

                                                    //raw
                                                    bc.raw2 = raw;
                                                    bc.rawMid();
                                                    //b.c.Add(bc);

                                                    //}
                                                    //else
                                                    //{
                                                    //    r.ReadBytes(16);
                                                    //}
                                                }
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        errorFile = "ポーズ「" + s + "」로드하는 동안 오류가 발생했습니다";
                        return false;
                    }
                }


                // 파일로 출력
                int si = get00000000byIntMid(s, s2);
                Debug.Log("String si " + si);
                using (BinaryWriter w = new BinaryWriter(File.Create(getPoseDataPath(true) + anmName + "_" + si.ToString("D8") + ".anm")))
                {
                    try
                    {
                        w.Write(header);
                        foreach (BoneDataA a in bda)
                        {
                            w.Write(a.outputABinary());
                        }
                        w.Write((byte)0);
                        w.Write((byte)0);
                        w.Write((byte)0);
                    }
                    catch (Exception)
                    {
                        errorFile = "モーション「" + anmName + ".anm」내보내기 중에 오류가 발생했습니다";
                        return false;
                    }
                }

            }

            return true;
        }

        private static int get00000000byInt(String s)
        {
            String cut = s.Substring(anmName.Length + 1, 8);
            int t = 0;
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    t = t * 10 + int.Parse(cut.Substring(i, 1));
                }
            }
            catch (Exception)
            {
                t = -1;
            }
            return t;
        }

        private static int get00000000byIntMid(String s, String s2)
        {
            String cut = s.Substring(anmName.Length + 1, 8);
            String cut2 = s2.Substring(anmName.Length + 1, 8);
            int t = 0;
            int t2 = 0;
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    t = t * 10 + int.Parse(cut.Substring(i, 1));
                }
                for (int i = 0; i < 8; i++)
                {
                    t2 = t2 * 10 + int.Parse(cut2.Substring(i, 1));
                }
                t = (t + t2) / 2;
            }
            catch (Exception)
            {
                t = -1;
            }
            return t;
        }

    }
}
