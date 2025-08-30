using ElShrine.EOption;
using System.Drawing;


namespace ElShrine.EGraphic
{
    public abstract class GraphicsPainterBase
    {
        protected static DrawingOption DrawingOption => DrawingOption.GetInstance();
        protected virtual void Draw(Graphics graphics) { }
        protected static void OffsetDraw(Action<Graphics> action, Graphics graphics, ref PointF offset)
        {
            graphics.TranslateTransform((float)offset.X, (float)offset.Y);
            action.Invoke(graphics);
            graphics.TranslateTransform((float)-offset.X, (float)-offset.Y);
        }
        protected static void RotateDraw(Action<Graphics> action, Graphics graphics, ref PointF rotateCentre, double radian)
        {
            float angle = (float)(radian / double.Pi * 180d);
            OffsetDraw((g) => g.RotateTransform(angle), graphics, ref rotateCentre);
            action.Invoke(graphics);
            OffsetDraw((g) => g.RotateTransform(-angle), graphics, ref rotateCentre);
        }
        protected static void ScaleDraw(Action<Graphics> action, Graphics graphics, SizeF scale)
        {
            graphics.ScaleTransform(scale.Width, scale.Height);
            action.Invoke(graphics);
            graphics.ScaleTransform(1 / scale.Width, 1 / scale.Height);
        }
        
        [Obsolete("NOT USE THIS to lines")]
        private sealed class CurveData(int maxDataCount)
        {
            public readonly PointF[] Data = new PointF[2 * maxDataCount];
            public readonly int Max = maxDataCount;
            public int startIndex = maxDataCount;//mdc -1 frist
            public int endIndex = maxDataCount - 1;//mdc frist
            public int Length => endIndex - startIndex + 1;
            public void SetEnd(ref PointF pointF) => Data[++endIndex] = pointF;
            public void SetStart(ref PointF pointF) => Data[--startIndex] = pointF;
        }
        [Obsolete("NOT USE THIS to lines")]
        protected static void MergedDrawLines(Graphics graphics, ref PointF[] p1s, ref PointF[] p2s, Pen pen)
        {
            CurveData cd = new(10);
            PointF p = new(1, 1);
            cd.SetStart(ref p);
            cd.SetStart(ref p);
            cd.SetStart(ref p);
            try
            {
                if (p1s.Length != p2s.Length) throw new($"the param array{nameof(p1s)} and {nameof(p2s)} should be length equated.");
                else
                {
                    List<CurveData> curveDatas = []; int length = p1s.Length;
                    for (int i = 0; i < length; i++)
                    {
                        bool foundHead = false, foundTail = false;
                        int headCurveIndex = -1, tailCurveIndex = -1;
                        bool headIsCurveHead = false, tailIsCurveTail = false;
                        for (int j = curveDatas.Count - 1; j >= 0; j--)
                        {
                            CurveData curve = curveDatas[j];
                            if (foundHead && foundTail) break;
                            if (!foundHead)
                            {
                                if (p1s[i] == curve.Data[curve.startIndex])
                                {
                                    foundHead = true;
                                    headCurveIndex = j;
                                    headIsCurveHead = true;
                                }
                                else if (p1s[i] == curve.Data[curve.endIndex])
                                {
                                    foundHead = true;
                                    headCurveIndex = j;
                                    headIsCurveHead = false;
                                }
                            }
                            if (!foundTail)
                            {
                                if (p2s[i] == curve.Data[curve.startIndex])
                                {
                                    foundTail = true;
                                    tailCurveIndex = j;
                                    tailIsCurveTail = true;
                                }
                                else if (p2s[i] == curve.Data[curve.endIndex])
                                {
                                    foundTail = true;
                                    tailCurveIndex = j;
                                    tailIsCurveTail = false;
                                }
                            }
                        }

                        if (foundHead && foundTail)
                        {
                            CurveData headMathcedCurve = curveDatas[headCurveIndex], tailMathcedCurve = curveDatas[tailCurveIndex];
                            if (headIsCurveHead && tailIsCurveTail)
                            {
                                //t p2 p1 t 
                                for (int x = 0; x < headMathcedCurve.Length; x++)
                                {
                                    tailMathcedCurve.SetEnd(ref headMathcedCurve.Data[headMathcedCurve.startIndex + x]);
                                }
                                curveDatas.Remove(headMathcedCurve);
                            }
                            else if (headIsCurveHead)
                            {
                                //t.rev p2 p1 h
                                for (int x = 0; x < tailMathcedCurve.Length; x++)
                                {
                                    headMathcedCurve.SetEnd(ref tailMathcedCurve.Data[tailMathcedCurve.endIndex - x]);
                                }
                                curveDatas.Remove(tailMathcedCurve);
                            }
                            else if (tailIsCurveTail)
                            {
                                //t p2 p1 h.rev
                                for (int x = 0; x < headMathcedCurve.Length; x++)
                                {
                                    tailMathcedCurve.SetEnd(ref headMathcedCurve.Data[headMathcedCurve.endIndex - x]);
                                }
                                curveDatas.Remove(headMathcedCurve);
                            }
                            else
                            {
                                //t.rev p2 p1 h.rev
                                for (int x = 0; x < tailMathcedCurve.Length; x++)
                                {
                                    headMathcedCurve.SetEnd(ref tailMathcedCurve.Data[tailMathcedCurve.startIndex + x]);
                                }
                                curveDatas.Remove(tailMathcedCurve);
                            }
                        }
                        else if (foundHead)
                        {
                            if (headIsCurveHead) curveDatas[headCurveIndex].SetStart(ref p2s[i]);
                            else curveDatas[headCurveIndex].SetEnd(ref p2s[i]);
                        }
                        else if (foundTail)
                        {
                            if (tailIsCurveTail) curveDatas[tailCurveIndex].SetStart(ref p1s[i]);
                            else curveDatas[tailCurveIndex].SetEnd(ref p1s[i]);
                        }
                        else
                        {
                            CurveData data = new(length);
                            data.SetStart(ref p1s[i]);
                            data.SetEnd(ref p2s[i]);
                            curveDatas.Add(data);
                        }
                    }
                    foreach (CurveData curveData in curveDatas)
                    {
                        PointF[] points = curveData.Data.Slice(curveData.startIndex, curveData.endIndex);
                        graphics.DrawLines(pen, points);
                    }
                }
            }
            catch
            {

            }

        }
    }
}
