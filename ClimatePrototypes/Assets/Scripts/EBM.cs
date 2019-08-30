using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using Extreme.Mathematics.Calculus.OrdinaryDifferentialEquations;

public static class EBM
{
	public static float A = 193;
	public static readonly float B = 2.1f;
	public static readonly float cw = 9.8f;
	public static readonly float D = 0.6f;

	public static readonly float S0 = 420;
	public static readonly float S2 = 240;
	public static readonly float a0 = 0.7f;
	public static readonly float a2 = 0.1f;
	public static readonly float aI = 0.4f;
	public static readonly float F = 0;

	public static readonly int bands = 3;

	public static readonly float Lv = 2500000;
	public static readonly float cp = 1004.6f;
	public static readonly float Rh = 0.8f;
	public static readonly float Ps = 100000;

	public static class simple
	{
		public static readonly float dx = 1f / (bands - 1f);
		public static Vector<float> x = Vector<float>.Build.Dense(bands, i => i++ * dx);

		public static Vector<float> simpleS = S0 - S2 * x.PointwisePower(2);
		public static Vector<float> aw = a0 - a2 * x.PointwisePower(2);

		public static Extreme.Mathematics.Vector<double> odefunc(double t, Extreme.Mathematics.Vector<double> y, Extreme.Mathematics.Vector<double> dy)
		{
			return Extreme.Mathematics.Vector.Create<double>(odefunc(Vector<float>.Build.DenseOfEnumerable(y.Select(n => (float)n)), (float)t).Select(n => (double)n).ToArray());
		}

		public static Vector<float> odefunc(Vector<float> temp, float t, bool moist = false)
		{
			Vector<float> T = Vector<float>.Build.DenseOfVector(temp);

			Vector<float> alpha = T.PointwiseSign().PointwiseMultiply(aw).Map(x => x < 0 ? aI : x);
			Vector<float> C = alpha.PointwiseMultiply(simpleS) - A + F;

			if (moist)
			{
				Vector<float> qs = humidity(T, Ps);
				Vector<float> q = Rh * qs;
				// Debug.Log(T);
				T += Lv * q / cp;
				// Debug.Log(T);
			}

			Vector<float> Tdot = Vector<float>.Build.Dense(x.Count);
			for (int i = 1; i < bands - 1; i++)
				Tdot[i] = (D / dx / dx) * (1 - x[i] * x[i]) * (T[i + 1] - 2 * T[i] + T[i - 1]) - (D * x[i] / dx) * (T[i + 1] - T[i - 1]);
			Tdot[0] = D * 2 * (T[1] - T[0]) / dx / dx;
			Tdot[Tdot.Count - 1] = -D * 2 * x[x.Count - 1] * (T[T.Count - 1] - T[T.Count - 2]) / dx;
			// Debug.Log(Tdot);
			Vector<float> f = (Tdot + C - B * temp) / cw;
			// Debug.Log(String.Join(" ", f));
			return f;
		}
		public static Vector<float> humidity(Vector<float> temp, float press)
		{
			float es0 = 610.78f;
			float t0 = 273.16f;
			float Rv = 461.5f;
			float ep = 0.622f;
			Vector<float> es = es0 * (-Lv / Rv * (1 / (temp + 273.15f) - 1 / t0)).PointwiseExp();
			Vector<float> qs = ep * es / press;
			// Debug.Log(es);
			// Debug.Log(qs);
			return qs;
		}

		//figure out odeint, it's kinda right at 100* steps
		public static float[] odeint(float[] temp, int steps, bool useMoisture = false)
		{
			float[] time = new float[steps].Select((T, k) => k * 30f / steps).ToArray();
			// Vector<float> t = Vector<float>.Build.DenseOfArray(temp);
			Extreme.Mathematics.Vector<double> t = Extreme.Mathematics.Vector.Create<double>(temp.Select(n => (double)n).ToArray());

			ClassicRungeKuttaIntegrator rk = new ClassicRungeKuttaIntegrator();
			// rk.DifferentialFunction = odefunc;
			rk.DifferentialFunction = Lorentz;
			rk.InitialTime = 0;
			rk.InitialValue = t;
			rk.StepSize = 30f / steps;

			for (int i = 0; i < steps; i++)
			{
				t = rk.Integrate(time[i]);
				Debug.Log(t);
			}
			// t += odefunc(t, time[i], useMoisture) / 100f;
			return t.Select(n => (float)n).ToArray();
		}

		//test ode
		static Extreme.Mathematics.Vector<double> Lorentz(double t, Extreme.Mathematics.Vector<double> y, Extreme.Mathematics.Vector<double> dy)
		{
			if (dy == null)
				dy = Extreme.Mathematics.Vector.Create<double>(3);

			double sigma = 10.0;
			double beta = 8.0 / 3.0;
			double rho = 28.0;

			dy[0] = sigma * (y[1] - y[0]);
			dy[1] = y[0] * (rho - y[2]) - y[1];
			dy[2] = y[0] * y[1] - beta * y[2];

			return dy;
		}
	}

	public static class fast
	{
		public static readonly int nt = 5;
		public static readonly int dur = 100;
		public static readonly float dt = 1f / nt;
		public static readonly float dx = 1f / bands;
		public static Vector<float> x = Vector<float>.Build.Dense(bands, i => dx / 2 + i++ * dx);
		public static Vector<float> xb = Vector<float>.Build.Dense(bands, i => ++i * dx);

		public static Vector<float> lam = D / dx / dx * (1 - xb.PointwisePower(2));
		public static Vector<float> L1 = Vector<float>.Build.Dense(bands, i => i == 0 ? 0 : -lam[i++ - 1]);
		public static Vector<float> L2 = Vector<float>.Build.Dense(bands, i => i >= bands ? 0 : -lam[i++]);
		public static Vector<float> L3 = -L1 - L2;
		public static Matrix<float> d3 = Matrix<float>.Build.DiagonalOfDiagonalVector(L3);
		public static Matrix<float> d2 = new Func<Matrix<float>>(() =>
		{
			Matrix<float> mat = Matrix<float>.Build.Dense(bands, bands, 0);
			mat.SetSubMatrix(0, 1, Matrix<float>.Build.DiagonalOfDiagonalVector(L2.SubVector(0, bands - 1)));
			return mat;
		})();
		public static Matrix<float> d1 = new Func<Matrix<float>>(() =>
		{
			Matrix<float> mat = Matrix<float>.Build.Dense(bands, bands, 0);
			mat.SetSubMatrix(1, 0, Matrix<float>.Build.DiagonalOfDiagonalVector(L1.SubVector(1, bands - 1)));
			return mat;
		})();

		public static Matrix<float> diffop = -d3 - d2 - d1;
		public static Vector<float> simpleS = S0 - S2 * x.PointwisePower(2);
		public static Vector<float> aw = a0 - a2 * x.PointwisePower(2);
		public static Vector<float> T = Vector<float>.Build.Dense(bands, 10);
		public static Matrix<float> allT = Matrix<float>.Build.Dense(bands, dur * nt, 0);
		public static Vector<float> t = Vector<float>.Build.Dense(dur * nt, i => i++ / nt);
		public static readonly Matrix<float> I = Matrix<float>.Build.DenseIdentity(bands);
		public static Matrix<float> invMat = (I + dt / cw * (B * I - diffop)).Inverse();
	}

	public static void printTest()
	{
		Debug.Log(fast.diffop);
		Debug.Log((fast.I + fast.dt / cw * (B * fast.I - fast.diffop)));

		Debug.Log(fast.invMat);
	}

}