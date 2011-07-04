/*
 * BSD Licence:
 * Copyright (c) 2007 Siddharth Jain [ morpheus@berkeley.edu ]
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the 
 * documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the <ORGANIZATION> nor the names of its contributors
 * may be used to endorse or promote products derived from this software
 * without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
 * DAMAGE.
 */

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Mapack;

namespace Sid
{
    /// <summary>
    /// The WA motion detector
    /// </summary>
    public class WAdetector
    {
        private const int MAXLEVELS = 5;

        // [level (in pyramids)][sensor orientation (theta)][omega_t][omega_y,omega_x]
        private ComplexF[][][][,] _M = new ComplexF[MAXLEVELS][][][,];
        private int _N, _W1, _W2, _L;
        private double _fr;

        public WAdetector()
        {

        }

        #region comment
        /// <summary>
        /// 
        /// </summary>
        /// <param name="N"># of orientations e.g. 10 in WA paper
        /// 0 to 360 degrees in steps of 36 degrees</param>
        /// <param name="W1"># of rows i.e. length in y dimension</param>
        /// <param name="W2"># of cols i.e. length in x dimension</param>
        /// <param name="L">length in time dimension</param>
        /// <param name="fr">frame rate of input e.g. 100Hz</param> 
        #endregion

        private void CreateM0(int N, int W1, int W2, int L, double fr)
        {
            _N = N;
            _W1 = W1;
            _W2 = W2;
            _L = L / 2 + 1;
            _fr = fr;
            _M = new ComplexF[MAXLEVELS][][][,];

            _M[0] = new ComplexF[N][][,];
            for (int ctr = 0; ctr < N; ctr++)
            {
                _M[0][ctr] = new ComplexF[_L][,];
                for (int ctr2 = 0; ctr2 < _L; ctr2++)
                    _M[0][ctr][ctr2] = new ComplexF[W1, W2];
            }

            double zeta = 0.9, tau1 = 0.004, tau2 = 0.0053;
            int nn1 = 9, nn2 = 10;
            double s0 = 0.5;
            double lambda0 = 0.795 / s0;
            double a0 = (Math.PI * lambda0) * (Math.PI * lambda0);
            for (int i = 0; i < N; i++)
            {
                double theta = 36.0 * Math.PI * i / 180.0;
                double sx = s0 * Math.Cos(theta);
                double sy = s0 * Math.Sin(theta);
                for (int j = 0; j < W1; j++)
                {
                    double v = -1 + 2.0 / W1 * j;
                    double t1 = (sy - v) * (sy - v);
                    double t2 = (sy + v) * (sy + v);
                    for (int k = 0; k < W2; k++)
                    {
                        double u = -1 + 2.0 / W2 * k;
                        double a1 = (sx - u) * (sx - u) + t1;
                        double a2 = (sx + u) * (sx + u) + t2;
                        double a3 = Math.Sign(sx * u + sy * v);
                        double a7 = Math.Exp(-a0 * a1) + Math.Exp(-a0 * a2);
                        for (int l = 0; l < _L; l++)
                        {
                            double w = -1 + 2.0 / L * l;
                            double b1 = Math.Sign(w);
                            double omega = w * Math.PI * fr;
                            ComplexF f1 = ComplexMath.Pow(1.0 + ComplexF.I * omega * tau1, -nn1);
                            ComplexF f2 = ComplexMath.Pow(1.0 + ComplexF.I * omega * tau2, -nn2);
                            ComplexF f = f1 - zeta * f2;
                            _M[0][i][l][j, k] = a7 * (1 - a3 * b1) * f;
                        }
                    }
                }
            }
        }

        private ComplexF[][][,] WASu(ComplexF[][][,] M)
        {
            ComplexF[][][,] f = new ComplexF[M.Length][][,];

            for (int i = 0; i < M.Length; i++)
            {
                f[i] = new ComplexF[M[0].Length][,];
                for (int j = 0; j < M[0].Length; j++)
                {
                    f[i][j] = new ComplexF[M[0][0].GetLength(0) / 2, M[0][0].GetLength(1) / 2];
                    for (int k = 0; k < M[0][0].GetLength(0); k += 2)
                        for (int l = 0; l < M[0][0].GetLength(1); l += 2)
                            f[i][j][k / 2, l / 2] = M[i][j][k, l];
                }
            }
            return f;
        }

        private ComplexF[, ,] WAShrink(ComplexF[, ,] C)
        {
            int l1 = C.GetLength(0);
            int l2 = C.GetLength(1);
            int l3 = C.GetLength(2);

            ComplexF[, ,] f = new ComplexF[l1 / 2, l2 / 2, l3];
            for (int i = l1 / 4; i < 3 * l1 / 4; i++)
                for (int j = l2 / 4; j < 3 * l2 / 4; j++)
                    for (int k = 0; k < l3; k++)
                        f[i - l1 / 4, j - l2 / 4, k] = C[i, j, k];
            return f;
        }

        private ComplexF[][][,] WAComputeR(ComplexF[][][,] M, ComplexF[, ,] C)
        {
            int l1 = M.Length;
            int l2 = M[0].Length;
            int l3 = M[0][0].GetLength(0);
            int l4 = M[0][0].GetLength(1);
            ComplexF[][][,] f = new ComplexF[l1][][,];

            for (int i = 0; i < l1; i++)
            {
                f[i] = new ComplexF[l2][,];
                for (int j = 0; j < l2; j++)
                {
                    f[i][j] = new ComplexF[l3, l4];
                    for (int k = 0; k < l3; k++)
                        for (int l = 0; l < l4; l++)
                            f[i][j][k, l] = M[i][j][k, l] * C[k, l, j];
                }
            }

            return f;
        }


        /// <summary>
        /// Suppose input I is [64,64,..]
        /// then level 0 is optic flow at scale [64,64,..]
        /// level 1 is optic flow at scale [32,32,..]
        /// level 2 is optic flow at scale [16,16,..]
        /// Note that the velocity is returned in a left handed coordinate system. vx is +ve to the right
        /// and vy is +ve to the bottom
        /// </summary>
        /// <param name="I">dimensions of I must be power of 2</param>
        /// <param name="fr">sampling rate. units: Hz.
        /// Suggestion: Use default value of 80</param>
        /// <param name="N">number of orientations.</param>
        /// <param name="LevelsToCompute">an array having
        /// ascending order of levels 
        /// at which you want the optic flow to be computed
        /// e.g. suppose I only care about the optic flow information at
        /// levels 1 and 2 then LevelsToCompute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3.</param>
        /// <returns></returns>
        /// 
        private float[][, ,] _WAComputeOpticalFlow(ComplexF[, ,] I, double fr, int N, int[] LevelsToCompute)
        {
            int W1 = I.GetLength(0);
            int W2 = I.GetLength(1);
            int L = I.GetLength(2);
            if (W1 != _W1 || W2 != _W2 || L / 2 + 1 != _L || fr != _fr || _N != N)
                CreateM0(N, W1, W2, L, fr);


            float[][, ,] F = new float[LevelsToCompute.Length][, ,];
            int maxlevel = LevelsToCompute[LevelsToCompute.Length - 1];
            double k = 0.5;

            ComplexF[, ,] Io = (ComplexF[, ,])I.Clone();

            Fourier.FFT3(Io, FourierDirection.Forward);
            ComplexF[, ,] C = Fourier.fftshift3(Io);
            ComplexF[][][,] R = WAComputeR(_M[0], C);

            int i = 0;
            for (int ctr = 0; ctr <= maxlevel && ctr <= _M.Length - 2; ctr++) // modification: 20070402
            {
                if (ctr == LevelsToCompute[i])
                {
                    F[i] = WAComputeVelocity(R, k, fr);
                    if (i == LevelsToCompute.Length - 1)
                    {
                        return F;
                    }
                    i++;
                }
                if (_M[ctr + 1] == null)
                    _M[ctr + 1] = WASu(_M[ctr]);
                C = WAShrink(C);
                R = WAComputeR(_M[ctr + 1], C);
                k /= 2;
            }

            return F;
        }


        #region comment
        /// <summary>
        /// Computes the optical flow for a sequence of images contained in filename
        /// </summary>
        /// <param name="sampling_rate">sampling rate. units: Hz.
        /// Suggestion: use default value of 80</param>
        /// <param name="fd">frame duration specified in integer multiple of 1/sampling_rate.
        /// frame duration = fd/sampling_rate
        /// Frame duration is the length of time for which an image stays on the screen
        /// (or the retina if you will) before being replaced by the next image in the sequence.
        /// Suggestion: use default value of 2
        /// </param>
        /// <param name="M">the temporal window specified in integer multiple of 1/sampling_rate.
        /// M must be a power of 2.
        /// Remember at time time t the motion you see is based on what
        /// your eyes saw from t-T to t where T is approx. 200 ms.
        /// Suggestion: use default value of 16. With a sampling rate of 80Hz this gives a temporal window of
        /// 16/80=200ms </param>
        /// <param name="num_orientations">number of orientations. Higher num_orientations will create more sensors tuned
        /// to different orientations with possibly no increase in accuracy for very high num_orientations.
        /// Suggestion: Use default value of 10</param>
        /// <param name="levels_to_compute">spatial scales at which optical flow is to be computed.
        /// LevelsToCompute is an array having ascending order of levels 
        /// at which you want the optical flow to be computed.
        /// level 0 has the finest spatial scale and its spatial resolution equals spatial resolution of input
        /// level 1 is the 2nd finest scale and so on.
        /// If the input has a resolution of 64x64 pixels level 0 will have a resolution of 64x64,
        /// level 1 will have a resolution of 32x32 and so on. 
        /// e.g. suppose I only care about the optical flow information at
        /// levels 1 and 2 then levels_to_compute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3
        /// Suggestion:  In my experiments I normally use a level that gives a 32x32 resolution and it also must 
        /// not be higher than 3 e.g. if input has resolution of 512x512 pixels level 4 will give 32x32 resolution
        /// but since it is higher than 3 I would use 3. The level to choose is really somewhat application specific.
        /// Experiment and choose what works best for you. Lower levels are suited for detecting very small displacements
        /// and in fact cannot detect large displacements.
        /// Technical stuff: Increasing the level number simply increases the size of the 
        /// receptive field (RF) of the sensor. For optimal response the total displacement a particle
        /// encounters should be such that it spans across the RF of the sensor. 
        /// </param>
        /// <param name="filename">an array which contains a sequence of images which becomes input to the
        /// optical flow algorithm</param>
        /// <returns>the optical flow. f[i][y,x,0] is vx, f[i][y,x,1] is vy and f[i][y,x,2] is a weight associated with
        /// (vx,vy) at level i. While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        #endregion

        public float[][, ,] WAComputeOpticalFlow(int sampling_rate, int fd, int M, int num_orientations, int[] levels_to_compute, Bitmap Image1, Bitmap Image2)
        {

            M = Fourier.RoundUptoPowerOf2(M);
            int h = Image1.Height;
            int w = Image1.Width;
            ComplexF[, ,] I = new ComplexF[h, w, M];
            int m = -1;
            for (int i = 0; i < M; i++)
            {
                if (i % fd == 0)
                {
                    m++;
                    if (m > fd )
                        break;
                    WAdetector.ReadImage(Image1, I, i);
                }
                else
                {
                    for (int j = 0; j < h; j++)
                        for (int k = 0; k < w; k++)
                        {
                            I[j, k, i] = I[j, k, i - 1];
                        }
                }
            }
            return WAComputeOpticalFlow(I, sampling_rate, num_orientations, levels_to_compute);
        }

        #region comment
        /// <summary>
        /// computes optical flow for the sequence of images given in filename.
        /// uses default values of parameters. 
        /// </summary>
        /// <param name="filename">an array which contains a sequence of images which becomes input to the
        /// optical flow algorithm</param>
        /// <returns>the optical flow. f[y,x,0] is vx, f[y,x,1] is vy and f[y,x,2] is a weight associated with
        /// (vx,vy). While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        #endregion


        //public float[, ,] WAComputeOpticalFlow(string[] filename)
        public float[, ,] WAComputeOpticalFlow2(int sampling_rate, int fd, int M, int num_orientations, int[] levels_to_compute, Bitmap Image1, Bitmap Image2)
        {
            float[][, ,] f = WAComputeOpticalFlow(sampling_rate, fd, M, num_orientations, levels_to_compute, Image1, Image2);
            return f[0];
        }

        private int WAScore(double[] b, int index)
        {
            int L = b.Length;
            int m = L / 4;
            int p = (int)Math.Round((double)L / 2);
            int f = 0;	// this is the score
            for (int ctr = -m; ctr <= m; ctr++)
            {
                /*
                 * c# % operator is a little screwed up
                 * c# gives -2 % 10 = -2 instead of 8
                 */
                int a1 = Mod((index + ctr), L);
                int a2 = Mod((index + ctr + p), L);
                if (b[a1] > b[a2])
                    f++;
            }
            return f;
        }

        #region comment
        /// <summary>
        /// computation of velocity as per sec. 5F
        /// Note that the velocity is returned in a left handed coordinate system. vx is +ve to the right
        /// and vy is +ve to the bottom
        /// </summary>
        /// <param name="R">[orientation][omega_t][omega_y,omega_x]</param>
        /// <param name="k0">spatial frequency of sensor e.g. 1/2, 1/4, 1/8 units: (pixels)^(-1)</param>
        /// <param name="fr">frame rate e.g. 100Hz</param>
        /// <returns>[y,x,0] is vx [y,x,1] is vy [y,x,2] is strength of direction group response </returns>
        #endregion


        private float[, ,] WAComputeVelocity(ComplexF[][][,] R, double k0, double fr)
        {
            int s1 = R.Length;
            int s4 = R[0].Length;
            int L = 2 * (s4 - 1);
            int s2 = R[0][0].GetLength(0);
            int s3 = R[0][0].GetLength(1);

            float[, ,] F = new float[s2, s3, 3];

            for (int i1 = 0; i1 < s1; i1++)
                for (int i2 = 0; i2 < s4; i2++)
                {
                    R[i1][i2] = Fourier.ifftshift2(R[i1][i2]);
                    Fourier.FFT2(R[i1][i2], FourierDirection.Backward);
                }

            /*
             * at each location (y,x) consider the sensors at the different orientations
             * theta = {0, 36, 72,...}
             * at each orientation response of sensor should ideally be a sinusoidally varying fn. of time
             * however because of noise in practice the response will not a perfect sinusoid
             * we are interested in the sinusoid with the largest component 
             * to find this sinusoid
             * consider the fourier transform of sensor response as a fn. of time 
             * the frequency corresponding to maxima in power spectrum is stored in b3
             * i.e. b3(i) = the ``fundamental frequency'' wt for ith orientation which encodes velocity as per
             * the master equation wx*vx+wy*vy+wt=0; (wx,wy) = spatial frequencies of the sensor
             * b1(i) = value of power spectrum at frequency b3(i)
             * strength of direction group response is max(b1)
             */
            double[] b1 = new double[s1], b3 = new double[s1];
            int[] b2 = new int[s1];
            double max = -1;	// maximum value variable

            for (int y = 0; y < s2; y++)
                for (int x = 0; x < s3; x++)
                {
                    for (int ll = 0; ll < s1; ll++)
                    {
                        max = -1;
                        for (int ctr = 0; ctr < s4; ctr++)
                        {
                            double mag = R[ll][ctr][y, x].GetModulus();
                            if (mag > max)
                            {
                                max = mag;
                                b1[ll] = max;
                                b2[ll] = ctr;
                                b3[ll] = (-1 + 2.0 / L * b2[ll]) * fr / 2.0;
                            }
                        }
                    }

                    /*
                     * strength of direction group response will be max(b1)
                     */
                    max = -1;
                    for (int ctr = 0; ctr < s1; ctr++)
                    {
                        if (b1[ctr] > max)
                            max = b1[ctr];
                    }
                    F[y, x, 2] = (float)max;

                    /*
                    * now do velocity estimation (method is different than that
                    * described in paper). Try to find out the position of the 
                    * peak point in fig. 8
                    */
                    double v1 = -1; int v2 = -1;
                    for (int ctr = 0; ctr < s1; ctr++)
                    {
                        double score = WAScore(b1, ctr);
                        if (score > v1)
                        {
                            v1 = score;
                            v2 = ctr;
                        }
                    }

                    int v3 = s1 / 4;
                    int index = 0;
                    Matrix A = new Matrix(2 * v3 + 1, 2);
                    Matrix B = new Matrix(2 * v3 + 1, 1);

                    for (int ctr = -v3; ctr <= v3; ctr++)
                    {
                        int v4 = Mod((v2 + ctr), s1);
                        double theta = 2 * Math.PI * v4 / s1;
                        A[index, 0] = -Math.Cos(theta) * k0 * b1[v4];	// b1[v4] is simply the weight I am giving to this eqn.
                        A[index, 1] = -Math.Sin(theta) * k0 * b1[v4];
                        B[index, 0] = b3[v4] * b1[v4];
                        index++;
                    }

                    Matrix X = A.Solve(B);
                    F[y, x, 0] = (float)X[0, 0];
                    F[y, x, 1] = (float)X[1, 0];
                    Matrix e = A * X - B;
                }
            return F;
        }

        private int Mod(double x, double y)
        {
            int n = (int)Math.Floor(x / y);
            return (int)(x - n * y);
        }


        #region comment
        /// <summary>
        /// Compute optical flow for the spatiotemporal stimulus in I
        /// </summary>
        /// <param name="I">the input spatiotemporal stimulus
        /// I(y,x,n) is luminance of pixel (y,x) in frame n.
        /// If the y and x dimensions are not powers of 2 the function will pad with zeros to make them powers of 2
        /// however no such padding is done for the n dimension and it must be a power of 2.
        /// Suggestion: create 16 frames i.e. I.GetLength(2) = 16. With a sampling rate of 80Hz this
        /// gives a temporal size of 16/80 = 200ms.
        /// If you have just 2 image frames available to do optical flow computation 
        /// assuming the images are played with a frame duration of 2/80=25ms
        /// which is about optimal for motion perception this is what you need to do:
        /// I(:,:,0) = I(:,:,1) = frame 1
        /// I(:,:,2) = I(:,:,3) = frame 2
        /// I(:,:,4) to I(:,:,15) = 0
        /// If the 2 image frames are played with a frame duration of 8/80=100ms then you would do following:
        /// I(:,:,0) to I(:,:,7) = frame 1
        /// I(:,:,8) to I(:,:,15) = frame 2
        /// By trying these two cases you can see for yourself the important role frame duration plays in 
        /// motion perception. 
        /// </param>
        /// <param name="fr">sampling rate. units: Hz.
        /// temporal spacing between successive frames equals 1/fr i.e. I(:,:,i) and I(:,:,i+1) have a temporal spacing
        /// given by 1/fr.
        /// Suggestion: use default value of 80</param>
        /// <param name="N">number of orientations. Higher N will create more sensors tuned to different orientations
        /// with possibly no increase in accuracy for very high N.
        /// Suggestion: Use default value of 10</param>
        /// <param name="LevelsToCompute">spatial scales at which optical flow is to be computed.
        /// LevelsToCompute is an array having ascending order of levels 
        /// at which you want the optical flow to be computed.
        /// level 0 has the finest spatial scale and its spatial resolution equals spatial resolution of input
        /// level 1 is the 2nd finest scale and so on.
        /// If the input has a resolution of 64x64 pixels level 0 will have a resolution of 64x64,
        /// level 1 will have a resolution of 32x32 and so on. 
        /// e.g. suppose I only care about the optical flow information at
        /// levels 1 and 2 then LevelsToCompute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3
        /// Suggestion:  In my experiments I normally use a level that gives a 32x32 resolution and it also must 
        /// not be higher than 3 e.g. if input has resolution of 512x512 pixels level 4 will give 32x32 resolution
        /// but since it is higher than 3 I would use 3. The level to choose is really somewhat application specific.
        /// Experiment and choose what works best for you. Lower levels are suited for detecting very small displacements
        /// and in fact cannot detect large displacements.
        /// Technical stuff: Increasing the level number simply increases the size of the 
        /// receptive field (RF) of the sensor. For optimal response the total displacement a particle
        /// encounters should be such that it spans across the RF of the sensor. 
        /// </param>
        /// <returns>the optical flow. f[i][y,x,0] is vx, f[i][y,x,1] is vy and f[i][y,x,2] is a weight associated with
        /// (vx,vy) at level i. While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        #endregion


        public float[][, ,] WAComputeOpticalFlow(float[, ,] I, double fr, int N, int[] LevelsToCompute)
        {
            int M = I.GetLength(0);
            int P = I.GetLength(1);
            int Q = I.GetLength(2);
            ComplexF[, ,] data = new ComplexF[M, P, Q];
            for (int i = 0; i < M; i++)
                for (int j = 0; j < P; j++)
                    for (int k = 0; k < Q; k++)
                        data[i, j, k].Re = I[i, j, k];
            return WAComputeOpticalFlow(data, fr, N, LevelsToCompute);
        }


        #region comment
        /// <summary>
        /// Compute optical flow for the spatiotemporal stimulus in I
        /// </summary>
        /// <param name="I">the input spatiotemporal stimulus
        /// I(y,x,n) is luminance of pixel (y,x) in frame n.
        /// If the y and x dimensions are not powers of 2 the function will pad with zeros to make them powers of 2
        /// however no such padding is done for the n dimension and it must be a power of 2.
        /// Suggestion: create 16 frames i.e. I.GetLength(2) = 16. With a sampling rate of 80Hz this
        /// gives a temporal size of 16/80 = 200ms.
        /// If you have just 2 image frames available to do optical flow computation 
        /// assuming the images are played with a frame duration of 2/80=25ms
        /// which is about optimal for motion perception this is what you need to do:
        /// I(:,:,0) = I(:,:,1) = frame 1
        /// I(:,:,2) = I(:,:,3) = frame 2
        /// I(:,:,4) to I(:,:,15) = 0
        /// If the 2 image frames are played with a frame duration of 8/80=100ms then you would do following:
        /// I(:,:,0) to I(:,:,7) = frame 1
        /// I(:,:,8) to I(:,:,15) = frame 2
        /// By trying these two cases you can see for yourself the important role frame duration plays in 
        /// motion perception. 
        /// </param>
        /// <param name="fr">sampling rate. units: Hz.
        /// temporal spacing between successive frames equals 1/fr i.e. I(:,:,i) and I(:,:,i+1) have a temporal spacing
        /// given by 1/fr.
        /// Suggestion: use default value of 80</param>
        /// <param name="N">number of orientations. Higher N will create more sensors tuned to different orientations
        /// with possibly no increase in accuracy for very high N.
        /// Suggestion: Use default value of 10</param>
        /// <param name="LevelsToCompute">spatial scales at which optical flow is to be computed.
        /// LevelsToCompute is an array having ascending order of levels 
        /// at which you want the optical flow to be computed.
        /// level 0 has the finest spatial scale and its spatial resolution equals spatial resolution of input
        /// level 1 is the 2nd finest scale and so on.
        /// If the input has a resolution of 64x64 pixels level 0 will have a resolution of 64x64,
        /// level 1 will have a resolution of 32x32 and so on. 
        /// e.g. suppose I only care about the optical flow information at
        /// levels 1 and 2 then LevelsToCompute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3
        /// Suggestion:  In my experiments I normally use a level that gives a 32x32 resolution and it also must 
        /// not be higher than 3 e.g. if input has resolution of 512x512 pixels level 4 will give 32x32 resolution
        /// but since it is higher than 3 I would use 3. The level to choose is really somewhat application specific.
        /// Experiment and choose what works best for you. Lower levels are suited for detecting very small displacements
        /// and in fact cannot detect large displacements.
        /// Technical stuff: Increasing the level number simply increases the size of the 
        /// receptive field (RF) of the sensor. For optimal response the total displacement a particle
        /// encounters should be such that it spans across the RF of the sensor. 
        /// </param>
        /// <returns>the optical flow. f[i][y,x,0] is vx, f[i][y,x,1] is vy and f[i][y,x,2] is a weight associated with
        /// (vx,vy) at level i. While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        #endregion


        public float[][, ,] WAComputeOpticalFlow(double[, ,] I, double fr, int N, int[] LevelsToCompute)
        {
            int M = I.GetLength(0);
            int P = I.GetLength(1);
            int Q = I.GetLength(2);
            ComplexF[, ,] data = new ComplexF[M, P, Q];
            for (int i = 0; i < M; i++)
                for (int j = 0; j < P; j++)
                    for (int k = 0; k < Q; k++)
                        data[i, j, k].Re = (float)I[i, j, k];
            return WAComputeOpticalFlow(data, fr, N, LevelsToCompute);
        }

        #region comment
        /// <summary>
        /// Compute optical flow for the spatiotemporal stimulus in I
        /// </summary>
        /// <param name="I">the input spatiotemporal stimulus
        /// I(y,x,n) is luminance of pixel (y,x) in frame n.
        /// If the y and x dimensions are not powers of 2 the function will pad with zeros to make them powers of 2
        /// however no such padding is done for the n dimension and it must be a power of 2.
        /// Suggestion: create 16 frames i.e. I.GetLength(2) = 16. With a sampling rate of 80Hz this
        /// gives a temporal size of 16/80 = 200ms.
        /// If you have just 2 image frames available to do optical flow computation 
        /// assuming the images are played with a frame duration of 2/80=25ms
        /// which is about optimal for motion perception this is what you need to do:
        /// I(:,:,0) = I(:,:,1) = frame 1
        /// I(:,:,2) = I(:,:,3) = frame 2
        /// I(:,:,4) to I(:,:,15) = 0
        /// If the 2 image frames are played with a frame duration of 8/80=100ms then you would do following:
        /// I(:,:,0) to I(:,:,7) = frame 1
        /// I(:,:,8) to I(:,:,15) = frame 2
        /// By trying these two cases you can see for yourself the important role frame duration plays in 
        /// motion perception. 
        /// </param>
        /// <param name="fr">sampling rate. units: Hz.
        /// temporal spacing between successive frames equals 1/fr i.e. I(:,:,i) and I(:,:,i+1) have a temporal spacing
        /// given by 1/fr.
        /// Suggestion: use default value of 80</param>
        /// <param name="N">number of orientations. Higher N will create more sensors tuned to different orientations
        /// with possibly no increase in accuracy for very high N.
        /// Suggestion: Use default value of 10</param>
        /// <param name="LevelsToCompute">spatial scales at which optical flow is to be computed.
        /// LevelsToCompute is an array having ascending order of levels 
        /// at which you want the optical flow to be computed.
        /// level 0 has the finest spatial scale and its spatial resolution equals spatial resolution of input
        /// level 1 is the 2nd finest scale and so on.
        /// If the input has a resolution of 64x64 pixels level 0 will have a resolution of 64x64,
        /// level 1 will have a resolution of 32x32 and so on. 
        /// e.g. suppose I only care about the optical flow information at
        /// levels 1 and 2 then LevelsToCompute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3
        /// Suggestion:  In my experiments I normally use a level that gives a 32x32 resolution and it also must 
        /// not be higher than 3 e.g. if input has resolution of 512x512 pixels level 4 will give 32x32 resolution
        /// but since it is higher than 3 I would use 3. The level to choose is really somewhat application specific.
        /// Experiment and choose what works best for you. Lower levels are suited for detecting very small displacements
        /// and in fact cannot detect large displacements.
        /// Technical stuff: Increasing the level number simply increases the size of the 
        /// receptive field (RF) of the sensor. For optimal response the total displacement a particle
        /// encounters should be such that it spans across the RF of the sensor. 
        /// </param>
        /// <returns>the optical flow. f[i][y,x,0] is vx, f[i][y,x,1] is vy and f[i][y,x,2] is a weight associated with
        /// (vx,vy) at level i. While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        /// 
        #endregion

        public float[][, ,] WAComputeOpticalFlow(int[, ,] I, double fr, int N, int[] LevelsToCompute)
        {
            int M = I.GetLength(0);
            int P = I.GetLength(1);
            int Q = I.GetLength(2);
            ComplexF[, ,] data = new ComplexF[M, P, Q];
            for (int i = 0; i < M; i++)
                for (int j = 0; j < P; j++)
                    for (int k = 0; k < Q; k++)
                        data[i, j, k].Re = I[i, j, k];
            return WAComputeOpticalFlow(data, fr, N, LevelsToCompute);
        }

        #region comment
        /// <summary>
        /// Compute optical flow for the spatiotemporal stimulus in I
        /// </summary>
        /// <param name="I">the input spatiotemporal stimulus
        /// I(y,x,n).Re is luminance of pixel (y,x) in frame n.
        /// I(y,x,n).Im is always 0
        /// If the y and x dimensions are not powers of 2 the function will pad with zeros to make them powers of 2
        /// however no such padding is done for the n dimension and it must be a power of 2.
        /// Suggestion: create 16 frames i.e. I.GetLength(2) = 16. With a sampling rate of 80Hz this
        /// gives a temporal size of 16/80 = 200ms.
        /// If you have just 2 image frames available to do optical flow computation 
        /// assuming the images are played with a frame duration of 2/80=25ms
        /// which is about optimal for motion perception this is what you need to do:
        /// I(:,:,0) = I(:,:,1) = frame 1
        /// I(:,:,2) = I(:,:,3) = frame 2
        /// I(:,:,4) to I(:,:,15) = 0
        /// If the 2 image frames are played with a frame duration of 8/80=100ms then you would do following:
        /// I(:,:,0) to I(:,:,7) = frame 1
        /// I(:,:,8) to I(:,:,15) = frame 2
        /// By trying these two cases you can see for yourself the important role frame duration plays in 
        /// motion perception. 
        /// </param>
        /// <param name="fr">sampling rate. units: Hz.
        /// temporal spacing between successive frames equals 1/fr i.e. I(:,:,i) and I(:,:,i+1) have a temporal spacing
        /// given by 1/fr.
        /// Suggestion: use default value of 80</param>
        /// <param name="N">number of orientations. Higher N will create more sensors tuned to different orientations
        /// with possibly no increase in accuracy for very high N.
        /// Suggestion: Use default value of 10</param>
        /// <param name="LevelsToCompute">spatial scales at which optical flow is to be computed.
        /// LevelsToCompute is an array having ascending order of levels 
        /// at which you want the optical flow to be computed.
        /// level 0 has the finest spatial scale and its spatial resolution equals spatial resolution of input
        /// level 1 is the 2nd finest scale and so on.
        /// If the input has a resolution of 64x64 pixels level 0 will have a resolution of 64x64,
        /// level 1 will have a resolution of 32x32 and so on. 
        /// e.g. suppose I only care about the optical flow information at
        /// levels 1 and 2 then LevelsToCompute = int[]{1,2}
        /// If LevelsToCompute does not have proper entries
        /// fn. will crash. Moreover note that the maximum entry in LevelsToCompute
        /// can only be MAXLEVELS-2 which is 5-2=3
        /// Suggestion:  In my experiments I normally use a level that gives a 32x32 resolution and it also must 
        /// not be higher than 3 e.g. if input has resolution of 512x512 pixels level 4 will give 32x32 resolution
        /// but since it is higher than 3 I would use 3. The level to choose is really somewhat application specific.
        /// Experiment and choose what works best for you. Lower levels are suited for detecting very small displacements
        /// and in fact cannot detect large displacements.
        /// Technical stuff: Increasing the level number simply increases the size of the 
        /// receptive field (RF) of the sensor. For optimal response the total displacement a particle
        /// encounters should be such that it spans across the RF of the sensor. 
        /// </param>
        /// <returns>the optical flow. f[i][y,x,0] is vx, f[i][y,x,1] is vy and f[i][y,x,2] is a weight associated with
        /// (vx,vy) at level i. While displaying quiver plots I usually scale (vx,vy) by this weight.
        /// Note that the optical flow is returned in a left handed coordinate system meaning vx is +ve to
        /// the right and vy is +ve to the bottom</returns>
        #endregion


        public float[][, ,] WAComputeOpticalFlow(ComplexF[, ,] I, double fr, int N, int[] LevelsToCompute)
        {
            int h = I.GetLength(0);
            int w = I.GetLength(1);
            if (Fourier.IsPowerOf2(h) && Fourier.IsPowerOf2(w))
                return _WAComputeOpticalFlow(I, fr, N, LevelsToCompute);
            else
            {
                int h2 = Fourier.RoundUptoPowerOf2(h);
                int w2 = Fourier.RoundUptoPowerOf2(w);
                int dh = h2 - h;

                int dw = w2 - w;
                int x1 = dw / 2;
                int y1 = dh / 2;

                int len = I.GetLength(2);
                ComplexF[, ,] data = new ComplexF[h2, w2, len];
                for (int t = 0; t < len; t++)
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            data[y + y1, x + x1, t] = I[y, x, t];
                float[][, ,] f = _WAComputeOpticalFlow(data, fr, N, LevelsToCompute);
                float[][, ,] f2 = new float[LevelsToCompute.Length][, ,];
                int n = f[0].GetLength(2);
                for (int i = 0; i < LevelsToCompute.Length; i++)
                {
                    int k = Fourier.Pow2(LevelsToCompute[i]);
                    int h3 = h / k, w3 = w / k, h4 = h2 / k, w4 = w2 / k;
                    int offset_y = (h4 - h3) / 2, offset_x = (w4 - w3) / 2;
                    f2[i] = new float[h3, w3, f[0].GetLength(2)];
                    for (int y = 0; y < h3; y++)
                        for (int x = 0; x < w3; x++)
                            for (int j = 0; j < f[0].GetLength(2); j++)
                                f2[i][y, x, j] = f[i][y + offset_y, x + offset_x, j];
                }
                return f2;
            }
        }

        #region comment
        /// <summary>
        /// tested on c:/c#/optic_flow_test_data/blocks/blocks.1.gif
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        #endregion

        private static ComplexF[,] ReadImage8bppIndexed(Bitmap bmp)
        {
            ComplexF[,] data = new ComplexF[bmp.Height, bmp.Width];

            int bpp = 1;
            BitmapData bmpData = bmp.LockBits(new Rectangle(new Point(0, 0),
                new Size(bmp.Width, bmp.Height)), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int offset = bmpData.Stride - bmpData.Width * bpp;
            Color[] pal = bmp.Palette.Entries;
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < bmp.Height; y++, ptr += offset)
                    for (int x = 0; x < bmp.Width; x++, ptr += bpp)
                    {
                        int i = (int)(*(ptr));
                        /*
                         * if instead of using pal[i] here you use
                         * bmp.Palette.Entries[i] it takes much much longer
                         */
                        data[y, x].Re = ((float)pal[i].R +
                            (float)pal[i].G +
                            (float)pal[i].B) / 3.0f;
                    }
            }
            bmp.UnlockBits(bmpData);

            return data;
        }

        #region comment
        /// <summary>
        /// tested on c:/c#/optic_flow_test_data/blocks/blocks.1.gif
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        #endregion

        private static void ReadImage8bppIndexed(Bitmap bmp, ComplexF[, ,] data, int index)
        {
            int bpp = 1;
            BitmapData bmpData = bmp.LockBits(new Rectangle(new Point(0, 0),
                new Size(bmp.Width, bmp.Height)), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int offset = bmpData.Stride - bmpData.Width * bpp;
            Color[] pal = bmp.Palette.Entries;
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < bmp.Height; y++, ptr += offset)
                    for (int x = 0; x < bmp.Width; x++, ptr += bpp)
                    {
                        int i = (int)(*(ptr));
                        /*
                         * if instead of using pal[i] here you use
                         * bmp.Palette.Entries[i] it takes much much longer
                         */
                        data[y, x, index].Re = ((float)pal[i].R +
                            (float)pal[i].G +
                            (float)pal[i].B) / 3.0f;
                    }
            }
            bmp.UnlockBits(bmpData);
        }

        private static void ReadImage24bppRgb(Bitmap bmp, ComplexF[, ,] data, int index)
        {
            int bpp = 3;
            BitmapData bmpData = bmp.LockBits(new Rectangle(new Point(0, 0),
                new Size(bmp.Width, bmp.Height)), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int offset = bmpData.Stride - bmpData.Width * bpp;

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < bmp.Height; y++, ptr += offset)
                    for (int x = 0; x < bmp.Width; x++, ptr += bpp)
                    {
                        int b = (int)(*(ptr));
                        int g = (int)(*(ptr + 1));
                        int r = (int)(*(ptr + 2));
                        data[y, x, index].Re = ((float)b +
                            (float)g +
                            (float)r) / 3.0f;
                    }
            }
            bmp.UnlockBits(bmpData);
        }

        #region comment
        /// <summary>
        /// Reads the image in filename. As of now the function only supports files that 
        /// have a pixel format of 8bpp indexed.
        /// </summary>
        /// <param name="filename">name of the file to read</param>
        /// <returns>pixel values in a complex array (imaginary parts are all zero)</returns>
        #endregion


        public static ComplexF[,] ReadImage(string filename)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(filename);
            }
            catch
            {
                throw new Exception("ReadImage: unable to read file " + filename);
            }
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                return ReadImage8bppIndexed(bmp);
            else
                throw new Exception("ReadImage: unable to read file " + filename);
        }

        #region comment
        /// <summary>
        /// Reads the image in filename. As of now the function only supports files that 
        /// have a pixel format of 8bpp indexed or 24bpp RGB.
        /// </summary>
        /// <param name="filename">file to read</param>
        /// <param name="data">the pixel values are stored in data[:,:,index]</param>
        /// <param name="index">the pixel values are stored in data[:,:,index]</param> 
        #endregion


        public static void ReadImage(Bitmap filename, ComplexF[, ,] data, int index)
        {
            Bitmap bmp = filename;
            //try
            //{
            //    bmp = new Bitmap(filename);
            //}
            //catch
            //{
            //    throw new Exception("ReadImage: unable to read file " + filename);
            //}
            FillFrameRGB(bmp, data, index);
        }

        #region readingbmp
        private static void FillFrameRGB(Bitmap BmpImage, ComplexF[, ,] data, int index)
        {
            #region Read in RGB
            BitmapData bmpData = BmpImage.LockBits(new Rectangle(0, 0, BmpImage.Width, BmpImage.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, BmpImage.PixelFormat);
            byte[,] byteBluePixels = new byte[BmpImage.Height, BmpImage.Width];
            byte[,] byteGreenPixels = new byte[BmpImage.Height, BmpImage.Width];
            byte[,] byteRedPixels = new byte[BmpImage.Height, BmpImage.Width];
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                if (BmpImage.PixelFormat == PixelFormat.Format64bppArgb || BmpImage.PixelFormat == PixelFormat.Format64bppPArgb)
                {
                    int space = bmpData.Stride - BmpImage.Width * 8;
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 8;
                        }
                        p += space;
                    }
                }
                if (BmpImage.PixelFormat == PixelFormat.Format32bppArgb || BmpImage.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    int space = bmpData.Stride - BmpImage.Width * 4;
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 4;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    int space = bmpData.Stride - BmpImage.Width * 3;
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 3;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format16bppRgb555)
                {
                    int space = bmpData.Stride - BmpImage.Width * 2;
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 2;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int space = bmpData.Stride - BmpImage.Width;
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = p[0];
                            p++;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format4bppIndexed)
                {
                    int space = bmpData.Stride - ((BmpImage.Width / 2) + (BmpImage.Width % 2));
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            int e = ((j + 1) % 2 == 0) ? p[0] >> 4 : p[0] & 0x0F;
                            byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = p[e];
                            p += ((j + 1) % 2);
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    for (int i = 0; i < BmpImage.Height; i++)
                    {
                        for (int j = 0; j < BmpImage.Width; j++)
                        {
                            byte* ptr = (byte*)bmpData.Scan0 + (i * bmpData.Stride) + (j / 8);
                            byte b = *ptr;
                            byte mask = Convert.ToByte(0x80 >> (j % 8));
                            if ((b & mask) != 0)
                                byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = 255;
                            else
                                byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = 0;
                        }
                    }
                }
            }
            BmpImage.UnlockBits(bmpData);
            #endregion
            for (int y = 0; y < BmpImage.Height; y++)
                for (int x = 0; x < BmpImage.Width; x++)
                {
                    data[y, x, index].Re = ((float)byteRedPixels[y, x] +
                                            (float)byteGreenPixels[y, x] +
                                            (float)byteBluePixels[y, x]) / 3.0f;
                }
        }
        #endregion

        #region comment
        /// <summary>
        /// convert color image to gray scale (NOT TESTED)
        /// </summary>
        /// <param name="bmp0"></param>
        /// <returns></returns> 
        #endregion

        public static Bitmap ConvertToGrayscale(Bitmap bmp0)
        {
            Bitmap bmp1 = null; int bpp = 3;
            if (bmp0.PixelFormat == PixelFormat.Format24bppRgb)
                bpp = 3;
            else if (bmp0.PixelFormat == PixelFormat.Format32bppArgb)
                bpp = 4;
            else
                return bmp0;
            BitmapData bmpData0 = bmp0.LockBits(new Rectangle(new Point(0, 0),
                new Size(bmp0.Width, bmp0.Height)), ImageLockMode.ReadOnly, bmp0.PixelFormat);
            bmp1 = new Bitmap(bmp0.Width, bmp0.Height, bmp0.PixelFormat);
            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(new Point(0, 0),
                new Size(bmp1.Width, bmp1.Height)), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            ColorPalette pal = bmp1.Palette;
            for (int i = 0; i < pal.Entries.Length; i++)
                pal.Entries[i] = Color.FromArgb(i, i, i);
            bmp1.Palette = pal;	// set monochrome palette

            int offset0 = bmpData0.Stride - bmpData0.Width * bpp;
            int offset1 = bmpData1.Stride - bmpData1.Width;
            unsafe
            {
                byte* ptr0 = (byte*)bmpData0.Scan0;
                byte* ptr1 = (byte*)bmpData1.Scan0;
                for (int y = 0; y < bmp0.Height; y++, ptr0 += offset0, ptr1 += offset1)
                    for (int x = 0; x < bmp0.Width; x++, ptr0 += bpp, ptr1++)
                    {
                        *ptr1 = (byte)(*(ptr0) + *(ptr0 + 1) + *(ptr0 + 2) / 3);
                    }
            }
            bmp0.UnlockBits(bmpData0);
            bmp1.UnlockBits(bmpData1);
            return bmp1;
        }

        private static float QuiverComputeMaxValue(float[, ,] data)
        {
            float max = -1;
            for (int y = 0; y < data.GetLength(0); y++)
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    float f = (float)Math.Sqrt(data[y, x, 0] * data[y, x, 0] + data[y, x, 1] * data[y, x, 1]);
                    if (f > max)
                        max = f;
                }
            return max;
        }

        private static void DrawArrow(System.Drawing.Graphics g, float x, float y, float vx, float vy)
        {
            Pen pen = new Pen(Color.Black);
            float k = 0.15f;
            float c = 0.70710678118654752440084436210485f; // cos(pi/2)
            float s = 0.70710678118654752440084436210485f; // sin(pi/2)
            // rotate (vx,vy) by +pi/2 and -pi/2
            float v1x = c * vx - s * vy;
            float v1y = s * vx + c * vy;
            float v2x = c * vx + s * vy;
            float v2y = -s * vx + c * vy;
            float x2 = x + vx, y2 = y + vy;
            g.DrawLine(pen, x, y, x2, y2);
            g.DrawLine(pen, x2, y2, x2 - v1x * k, y2 - v1y * k);
            g.DrawLine(pen, x2, y2, x2 - v2x * k, y2 - v2y * k);
        }

        #region comment
        /// <summary>
        /// creates a bitmap that has a quiver plot of the data passed to the function.
        /// Use this function to visualize the optical flow.
        /// </summary>
        /// <param name="data">data[y,x,0] is x component
        /// data[y,x,1] is y component</param>
        /// <returns>bitmap that has the quiver plot</returns> 
        #endregion
        static int Counter = 0;
        public static float[, ,] Quiver(float[, ,] data)
        {
            int k1 = 640;		// default width/height of quiver plot
            int k2 = 64;		// default no. of flow vectors
            float k4 = 1.4f;	// controls length of arrows

            int h1 = data.GetLength(0);
            int w1 = data.GetLength(1);
            int w2, h2, nx, ny, nw, nh;
            float delta;

            if (Math.Max(w1, h1) < k1)
            {
                if (w1 > h1)
                {
                    w2 = k1; h2 = k1 * h1 / w1;
                }
                else
                {
                    h2 = k1; w2 = k1 * w1 / h1;
                }
            }
            else
            {
                w2 = w1; h2 = h1;
            }
            if (w1 > h1)
            {
                nw = Math.Min(w1, k2);
                delta = (float)w2 / (nw + 1);
                nh = (int)Math.Round((float)h2 / delta - 1);
            }
            else
            {
                nh = Math.Min(h1, k2);
                delta = (float)h2 / (nh + 1);
                nw = (int)Math.Round((float)w2 / delta - 1);
            }

            float max = QuiverComputeMaxValue(data);
            float k5 = k4 * delta / max;
            float fx = ((float)w1 - 1.0f) / (nw - 1.0f);
            float fy = ((float)h1 - 1.0f) / (nh - 1.0f);

            Bitmap bmp = new Bitmap(w2, h2);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            SolidBrush b = new SolidBrush(Color.White);
            g.FillRectangle(b, 0, 0, w2, h2);	// fill background with white
            b.Dispose();
            float x, y;
            float[, ,] WarpedData = data;
            for (ny = 0, y = delta; ny < nh && y < h2; ny++, y += delta)
                for (nx = 0, x = delta; nx < nw && x < w2; nx++, x += delta)
                {
                    int x1 = (int)Math.Round(fx * nx); int y1 = (int)Math.Round(fy * ny);
                    DrawArrow(g, x, y, data[y1, x1, 0] * k5, data[y1, x1, 1] * k5);
                    WarpedData[y1, x1, 0] = data[y1, x1, 0] * k5;
                    WarpedData[y1, x1, 1] = data[y1, x1, 1] * k5;
                }
            string Pw;
            Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Optical Flow From " + Counter + " to " + (Counter + 1) + ".bmp";
            Counter++;
            bmp.Save(Pw); // save the quiver plot
            g.Dispose();
            return WarpedData;
        }

        #region comment
        /// <summary>
        /// Saves the optical flow data in a filename.
        /// It simply writes the data in binary format and before writing the data it writes the dimensions of f.
        /// Use function ReadOpticalFlow to read the optical flow data
        /// </summary>
        /// <param name="f">optical flow data</param>
        /// <param name="filename">filename</param> 
        #endregion

        public static void SaveOpticalFlow(float[, ,] f, string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(f.GetLength(0));
            bw.Write(f.GetLength(1));
            bw.Write(f.GetLength(2));
            for (int i = 0; i < f.GetLength(0); i++)
                for (int j = 0; j < f.GetLength(1); j++)
                    for (int k = 0; k < f.GetLength(2); k++)
                        bw.Write(f[i, j, k]);
            bw.Close();
            fs.Close();
        }

        #region comment
        /// <summary>
        /// use this function to read the optical flow data that was saved using the function SaveOpticalFlow
        /// </summary>
        /// <param name="filename">name of the file to read</param>
        /// <returns>optical flow data</returns> 
        #endregion

        public static float[, ,] ReadOpticalFlow(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            int h = br.ReadInt32();
            int w = br.ReadInt32();
            int n = br.ReadInt32();
            float[, ,] f = new float[h, w, n];
            for (int i = 0; i < f.GetLength(0); i++)
                for (int j = 0; j < f.GetLength(1); j++)
                    for (int k = 0; k < f.GetLength(2); k++)
                        f[i, j, k] = br.ReadSingle();
            br.Close();
            fs.Close();
            return f;
        }
    }
}