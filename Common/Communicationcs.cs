using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public delegate T MyDelegate<T, K>(K value);//声明一个委托用来控制数据传输
 
    /// <summary>
    /// 装载数据的容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class Communicationcs<T,K>
    {
        public List<T> simMaPos=new List<T>();//机械坐标
        public List<T> simCurPos = new List<T>();//当前坐标
        public List<T> simWorkPos = new List<T>();//工件坐标
        public List<T> simDisPos = new List<T>();//机械坐标
        public List<T> simSpMionitor=new List<T>() ;//主轴监视数据
    }

    
}
