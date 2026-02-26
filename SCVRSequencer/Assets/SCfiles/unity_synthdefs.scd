//setup
(

// s.options.outDevice = "Oculus";
s.options.sampleRate = 44100;

s.waitForBoot({
	fork{


		//Loading sound sources into buffers

		var paths;
		b = ();
		paths = PathName(PathName(thisProcess.nowExecutingPath).parentPath ++ "samples/");

		paths.entries
		.select{|it| it.extension == "wav"} //per evitare messaggi in console - fallita apertura file .meta
		.do{|path|
			if(path.fileNameWithoutExtension == "voice")
			{b[path.fileNameWithoutExtension.asSymbol] = Buffer.readChannel(s, path.fullPath, channels:[0])}
			{b[path.fileNameWithoutExtension.asSymbol] = Buffer.read(s, path.fullPath)}
		};


		s.sync;

		//ambisonics
		~enc = FoaEncoderMatrix.newDirection(0,0);
		~encStereo = FoaEncoderKernel.newSuper(2048);
		~dec = FoaDecoderMatrix.newStereo(90.degrad /2, 0.5);

		//versione headphones
		// ~enc = FoaEncoderKernel.newSpread(12, 2048);
		// ~dec = FoaDecoderKernel.newListen(1002);

		/* id per newlisten
		1002	1003	1004	1005	1006	1007	1008	1009	1012	1013
		1014	1015	1016	1017	1018	1020	1021	1022	1023	1025
		1026	1028	1029	1030	1031	1032	1033	1034	1037	1038
		1039	1040	1041	1042	1043	1044	1045	1046	1047	1048
		1049	1050	1051	1052	1053	1054	1055	1056	1057	1058
		1059
		*/

		s.sync;

		SynthDef(\hat,
			{
				var	sig, buf = \buf.kr(0);
				sig = PlayBuf.ar(2,buf, BufRateScale.ir(buf) * \rate.kr(1), startPos: \spos.kr(0), doneAction:2);
				sig = Latch.ar(sig, Impulse.ar(\bitcrush.kr(s.sampleRate/2)));
				sig = sig * \amp.kr(0.5);


				sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

				sig = FoaRTT.ar(sig,
					rotAngle: \rotate.kr(0).degrad ,
					tilAngle: 0,
					tumAngle: \tumble.kr(0).degrad
				);
				sig = sig * \distanza.kr(1);
				sig = FoaDecode.ar(sig, ~dec);

				Out.ar(0, Balance2.ar(sig[0], sig[1], \pan.kr(0)))
			}
		).add;


		SynthDef(\kick,
			{
				var	sig, buf = \buf.kr(0);

				sig = PlayBuf.ar(2,buf, BufRateScale.ir(buf) * \rate.kr(1), startPos: \spos.kr(0), doneAction:2);
				sig = Latch.ar(sig, Impulse.ar(\bitcrush.kr(s.sampleRate/2)));
				sig = sig * \amp.kr(0.5);
				sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

				sig = FoaRTT.ar(sig,
					rotAngle: \rotate.kr(0).degrad ,
					tilAngle: 0,
					tumAngle: \tumble.kr(0).degrad
				);
				sig = sig * \distanza.kr(1);
				sig = FoaDecode.ar(sig, ~dec);
				Out.ar(0, Balance2.ar(sig[0], sig[1], \pan.kr(0)))
			}
		).add;

		SynthDef(\snare,
			{
				var	sig, buf = \buf.kr(0);
				sig = PlayBuf.ar(2,buf, BufRateScale.ir(buf) * \rate.kr(1), startPos: \spos.kr(0), doneAction:2);
				sig = Latch.ar(sig, Impulse.ar(\bitcrush.kr(s.sampleRate/2)));
				sig = sig * \amp.kr(0.5);
				sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

				sig = FoaRTT.ar(sig,
					rotAngle: \rotate.kr(0).degrad ,
					tilAngle: 0,
					tumAngle: \tumble.kr(0).degrad
				);
				sig = sig * \distanza.kr(1);
				sig = FoaDecode.ar(sig, ~dec);
				Out.ar(0, Balance2.ar(sig[0], sig[1], \pan.kr(0)))
			}
		).add;

		SynthDef(\clap,
			{
				var	sig, buf = \buf.kr(0);
				sig = PlayBuf.ar(2,buf, BufRateScale.ir(buf) * \rate.kr(1),startPos: \spos.kr(0), doneAction:2);
				sig = Latch.ar(sig, Impulse.ar(\bitcrush.kr(s.sampleRate/2)));
				sig = sig * -6.dbamp;
				sig = sig * \amp.kr(0.5);
				sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

				sig = FoaRTT.ar(sig,
					rotAngle: \rotate.kr(0).degrad ,
					tilAngle: 0,
					tumAngle: \tumble.kr(0).degrad
				);
				sig = sig * \distanza.kr(1);
				sig = FoaDecode.ar(sig, ~dec);
				Out.ar(0, Balance2.ar(sig[0], sig[1], \pan.kr(0)))
			}
		).add;

		s.sync;

		SynthDef(\pads, {
			var sig, freq, lfo, mod;
			freq = \freq.kr(400);
			lfo = ((LFNoise1.kr(0.1 ! 8) * \detune.kr(0.1).lag(0.1)) * XLine.kr(1,0.5, 1)).midiratio ;
			sig = Saw.ar(freq * lfo * [1, 0.5, 2, 0.4, 0.4, 2, 3/2, 1]) * [0, -2, -5, -5, -6, -3, -3, -4].dbamp;

			sig = LPF.ar(sig, \cutoff.kr(800).lag(0.5));
			//alt
			// sig = LPF.ar(sig, freq.expexp(100,1000,200,3000));
			// sig = RLPF.ar(sig, LFNoise2.kr(3!8).linexp(-1,1,100,4000),0.5);

			sig = Splay.ar(sig, \pan.kr(0).lag(0.1));
			sig = sig * 12.dbamp;
			// sig = sig * -10.dbamp; //senza visore
			sig = BLowShelf.ar(sig, 1000, 1, -10);
			sig = sig * Env.adsr(0.01, 0.1, \level.kr(1), \rel.kr(1)).ar(2, \gate.kr(1));
			sig = Limiter.ar(sig, 0.5);


			sig = FoaEncode.ar(sig, ~encStereo); //B-format (4ch)

			sig = FoaRTT.ar(sig,
				rotAngle: \rotate.kr(0).degrad ,
				tilAngle: 0,
				tumAngle: \tumble.kr(0).degrad
			);
			sig = sig * \distanza.kr(1);
			sig = FoaDecode.ar(sig, ~dec);

			Out.ar(0, sig * \amp.kr(1).lag(0.2))
		}).add;

		s.sync;

		SynthDef(\bass, {
			var sig, freq;
			freq = \freq.kr(261);
			sig = Pulse.ar(freq * [-0.05,0.05].midiratio, \width.kr(0.7));
			sig = DelayL.ar(sig, 0.2, {Rand(0,0.01)}!2).sum;
			sig = ((sig * -15.dbamp) *  Env.perc(0, 0.5).ar)
			+ (SinOsc.ar(freq) * 0.4 * Env.adsr(0.001, 0.1, \level.kr(1), \rel.kr(3), curve: -4).ar(2, \gate.kr(1)));
			sig = LPF.ar(sig , XLine.kr(7e3, 500, \sweepDur.kr(0.6)));
			sig = HPF.ar(sig,80);
			sig = Compander.ar(sig, sig, 0.5, 1, 0.5);
			//sig = sig *6.dbamp;
			sig = Limiter.ar(sig);

			sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

			sig = FoaRTT.ar(sig,
				rotAngle: \rotate.kr(0).degrad ,
				tilAngle: 0,
				tumAngle: \tumble.kr(0).degrad
			);
			sig = sig * \distanza.kr(1);
			sig = FoaDecode.ar(sig, ~dec); //2ch


			// sig = Pan2.ar(sig, \pan.kr(0));
			Out.ar(0, sig * \amp.kr(1));
		}).add;

		s.sync;

		SynthDef(\theremin,{
			var sig, fb, freq, modAmp, carFreq;
			fb = LocalIn.ar(1);
			carFreq = \carFreq.kr(100).lag(1);
			modAmp = \modAmp.kr(1).lag(1);
			sig = SinOsc.ar(carFreq + fb) * modAmp;
			sig = SinOsc.ar(sig + carFreq) * modAmp;
			sig = sig + CombC.ar(sig, 0.2, SinOsc.kr(0.1).range(0.001,0.01));
			sig = (sig*10.dbamp).tanh * modAmp;
			LocalOut.ar(sig);
			sig = sig / modAmp; //scalo tra -1 e 1 di nuovo
			sig = Limiter.ar(sig);
			sig = sig*10.dbamp;
			sig = LPF.ar(sig, 8e3);
			sig = sig * Env.asr(0.1,1,0.2).ar(2, \gate.kr(1));
			sig = FoaEncode.ar(sig, ~enc); //B-format (4ch)

			sig = FoaRTT.ar(sig,
				rotAngle: \rotate.kr(0).degrad ,
				tilAngle: 0,
				tumAngle: \tumble.kr(0).degrad
			);
			sig = sig * \distanza.kr(1);
			sig = FoaDecode.ar(sig, ~dec); //2ch

			//sig = Pan2.ar(sig, 0, 0.04);
			// sig = Limiter.ar(sig);
			Out.ar(0, sig * \amp.kr(1).lag(1) * 0.04);
		}).add;

		s.sync;

		SynthDef(\voice, {
			var sig, numeroPtr = 3, buf = \buf.kr(0);
			var trigFreq = \trigFreq.kr(20);
			// var automatico = LFNoise1.kr(5).range(0,0); //cambia range
			var trig = Impulse.ar(trigFreq);
			var rateMod = LFNoise1.ar(\rFreq.kr(0.7) ! numeroPtr).range(0, \rAmp.kr(0.05));
			var posMod = LFNoise1.ar(4!numeroPtr).range(0, numeroPtr.linlin(0,6, 1/10,1/3) * \diffusion.kr(1));
			sig = GrainBuf.ar(
				numChannels: 1,//2,
				trigger: trig,
				dur: \overlap.kr(5) / trigFreq,
				sndbuf: buf,
				rate: \rate.kr(1).lag(0.1) + rateMod,
				pos: \pos.kr(0) + posMod, //+ automatico, //disattivare comando pos se automatico è attivo
				interp: 2,
				//pan: SinOsc.kr(\panFreq.kr(1)).range(-1,1) * \diffusion.kr(1) //tra 0 e 1 (\panDiffusion)
			);
			sig = sig * Env.asr(0.1,1,0.2).ar(2, \gate.kr(1));
			sig = sig * 30.dbamp;
			sig = Splay.ar(sig);
			sig = Limiter.ar(sig);



			sig = FoaEncode.ar(sig, ~encStereo); //B-format (4ch)

			sig = FoaRTT.ar(sig,
				rotAngle: \rotate.kr(0).degrad ,
				tilAngle: 0,
				tumAngle: \tumble.kr(0).degrad
			);
			sig = sig * \distanza.kr(1);
			sig = FoaDecode.ar(sig, ~dec);

			Out.ar(0, sig * \amp.kr(1));

		}).add;

		s.sync;

		SynthDef(\piano, {
			var freq = \freq.kr(60).midicps;
			var sig = SinOsc.ar(freq * 3) * 650 * (0.1 + Env.perc(0.01, 0.5, curve: -1).ar);
			sig = SinOsc.ar(freq + sig)
			* 0.3
			* Env.perc(0.01, 0.5).ar(2);
			sig = Limiter.ar(sig,0.7);
			Out.ar(0, sig ! 2);
		}).add;

		SynthDef(\limiter, {
			var in = In.ar(0,2);
			Out.ar(0, Limiter.ar(in));
		}).add;

		s.sync;

		SynthDef(\reverb,{
			var sig = In.ar(0,2);

			sig = FreeVerb.ar(sig, \drywet.kr(0.0),
				\room.kr(0.85),
				\damp.kr(1));
			Out.ar(0, sig * \amp.kr(1));
		}).add;

		s.sync;
		SynthDef(\cue, {var buf = \buf.kr(0);
			var sig = PlayBuf.ar(1, buf, BufRateScale.ir(buf), doneAction:2);
			Out.ar(0, sig !2);
		}).add;

		s.sync;

		//
		(PathName(thisProcess.nowExecutingPath).parentPath++"unity_pattern_unico.scd").load;

		"fine----------".postln;

	}
});
)