using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using openCV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

namespace EMTEST
{
    public class EMTET
    {
        public EMTET() { }
        public void Trainn()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int N = 2000;
            int D = 10;
            int G = 10;

            EM Em = new EM();
            Matrix<int> labels = new Matrix<int>(N, 1);
            Matrix<float> featuresM = new Matrix<float>(N, D);
            for (int i = 0; i < N; i++)
                for (int j = 0; j < D; j++)
                    featuresM[i, j] = 100 * (float)r.NextDouble() - 50;

            EMParams pars = new EMParams();
            pars.CovMatType = Emgu.CV.ML.MlEnum.EM_COVARIAN_MATRIX_TYPE.COV_MAT_DIAGONAL;
            pars.Nclusters = G;
            pars.StartStep = Emgu.CV.ML.MlEnum.EM_INIT_STEP_TYPE.START_AUTO_STEP;
            pars.TermCrit = new MCvTermCriteria(100, 1.0e-6);

            Em.Train(featuresM, null, pars, labels);
            Matrix<double> Means = Em.GetMeans();
            Matrix<double>[] Covariance = Em.GetCovariances();
        }
    }
}
