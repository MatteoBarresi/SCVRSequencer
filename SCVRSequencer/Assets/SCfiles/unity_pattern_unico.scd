(
var bass_ampBus, bass_widthBus, bass_spaceBus, bass_chain;
var bass_ampFunc, bass_widthFunc, bass_spaceFunc;
var pads_ampBus, pads_cutoffBus, pads_panBus, pads_spaceBus, pads_chain;
var pads_ampFunc, pads_cutoffFunc, pads_panFunc, pads_spaceFunc;


~drums = {|name|

	var spaceBus = [Bus.control(s, 1), Bus.control(s, 1), Bus.control(s, 1)];
	var spaceFunc, chain;
	spaceFunc = {
		Out.kr(spaceBus[0], \rotate.kr(0));
		Out.kr(spaceBus[1], \tumble.kr(0));
		Out.kr(spaceBus[2], \distanza.kr(1));
	}.play;



	//Pbindef((name++"pattern2").asSymbol, \rotate, spaceBus[0].asMap, \tumble, spaceBus[1].asMap).quant_(0);

	OSCdef((name++"osc").asSymbol,{arg msg; //msg.postln;
		//fare switch(msg[1]) come pads e bass
		if(msg.size==2 &&  msg[1]==\play){
			msg.postln;

			chain = Pchain(Pbindef((name++"pattern2").asSymbol), Pbindef((name++"pattern").asSymbol)).play(t, quant: 4);
			// chain.play(t, quant: 4);
			Pbindef((name++"pattern2").asSymbol,
				\rotate, spaceBus[0].asMap,
				\tumble, spaceBus[1].asMap,
				\distanza, spaceBus[2].asMap,
			).quant_(0);
			// Pchain(Pbindef((name++"pattern2").asSymbol), Pbindef((name++"pattern").asSymbol)).quant_(4);
			// Pbindef((name++"pattern").asSymbol).quant_(4)
			//~unity.sendMsg('/ricevuto');
		}{if(msg.size == 2 && msg[1] == \free){ //sia i singoli pattern che la chain
			msg.postln;
			Pbindef((name++"pattern").asSymbol).stop;
			Pbindef((name++"pattern").asSymbol).clear;
			Pbindef((name++"pattern2").asSymbol).stop;
			Pbindef((name++"pattern2").asSymbol).clear;
			chain.clear;
			Pbindef((name++"pattern").asSymbol,
				\instrument, name.asSymbol,
				\trigger, Pseq(0!16,inf),
				\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
				\dur, 1/4,
				\buf, b[name.asSymbol]
				//\rotate, 0,//spaceBus[0].asMap,
				//\tumble, 0//spaceBus[1].asMap
			);
		/*	Pbindef((name++"pattern2").asSymbol,
				\rotate, spaceBus[0].asMap,
				\tumble, spaceBus[1].asMap
			).quant_(0).pause;*/
		};
		};
		if(msg.size==2 && msg[1]==\pause){
			// Pbindef((name++"pattern").asSymbol).quant_(0);
			// Pbindef((name++"pattern").asSymbol).pause;
			chain.pause;
			~pauseBeat = t.beats*4%16;
			~pauseBeat.postln;
		};
		if(msg.size==2 && msg[1]==\resume){msg.postln;
			chain.resume(t, Quant(4,(~pauseBeat+1).floor/4))

			// Pbindef((name++"pattern").asSymbol).resume(t, Quant(4,(~pauseBeat+1).floor/4))
		}{if(msg.size==2 && msg[1] == \resumeB){
			msg.postln;
			chain.resume(t, Quant(4))
		}};

		if(msg.size==3){//set (vanno mappati)
			if((msg[2]!=\start).and(msg[2]!=\stop)){
				switch(msg[1])
				{\amp}{Pbindef((name++"pattern").asSymbol, msg[1],
					Pif(Pkey(\trigger) > 0, msg[2].linlin(-0.1,1.1 ,0,1), Rest())).quant_(1)
				}
				{\bitcrush}{Pbindef((name++"pattern").asSymbol, msg[1],
					Pif(Pkey(\trigger) > 0, msg[2].linexp(-0.1,1, 8e3, s.sampleRate/2), Rest())).quant_(1)}
				{\rate}{Pbindef((name++"pattern").asSymbol, msg[1],
					Pif(Pkey(\trigger) > 0, msg[2].linlin(-0.1,1.1 ,-1,1), Rest()),
					\spos, Pif(Pkey(\rate) > 0, 0, b[name.asSymbol].numFrames-2)).quant_(1)}
				/*{\pan}{Pbindef((name++"pattern").asSymbol, msg[1],
				Pif(Pkey(\trigger) > 0, msg[2].linlin(-0.1,1.1 ,-1,1), Rest())).quant_(1)}
				*/
				//audio 3d - fare bus kr e mappare??
				{\rotate}{

					/////////////Pchain(Pbindef((name++"pattern").asSymbol), Pbindef((name++"pattern2").asSymbol, msg[1], msg[2].neg).quant_(1))

					spaceFunc.set(\rotate, msg[2].neg);
					// Pbindef((name++"pattern2").asSymbol, msg[1], msg[2].neg)//.quant_(1)
				}
				{\tumble}{
					// ----Pchain(Pbindef((name++"pattern").asSymbol), Pbindef((name++"pattern2").asSymbol, msg[1], msg[2]).quant_(1))

					spaceFunc.set(\tumble, msg[2]);
					// Pbindef((name++"pattern2").asSymbol, msg[1], msg[2])//.quant_(1)
				}
				{\distanza}{
					spaceFunc.set(\distanza, msg[2].round(0.01).linlin(0.1, 10, 1, 0.1));
					//("ampout " + msg[2].round(0.01).linlin(0.1, 6.5, 1, 0.01) ).postln;
				}
			}
		};
		if(msg.size > 3){ //pattern
			var quantValue;
			if(msg.last == \pino){quantValue = 0; "aaaaaaaaaaaa".postln}{quantValue = 4};
			msg.postln;
			Pbindef((name++"pattern").asSymbol,
				\instrument, name.asSymbol,
				\trigger, Pseq(msg[1..16],inf),
				\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
				\dur, 1/4,
				\buf, b[name.asSymbol]
		).quant_(quantValue/*4*/)};
},("/"++name).asSymbol);
};

~pauseBeat = nil;
~drums.("kick");
~drums.("hat");
~drums.("snare");
~drums.("clap");

//sequencer a 16 slot (non faccio costante globale)
NetAddr.disconnectAll;
~unity = NetAddr.new("127.0.0.1", 7000);
t = TempoClock(100/60);
t.schedAbs(0, {~unity.sendMsg('/beat', (t.beats*4%16 + 1));1/4});

//questa va mandata dopo aver avviato l'applicazione
b.keys.do{|instr| ~unity.sendMsg('/path', instr , b[instr].path)};

OSCdef(\pathosc, {arg msg; msg.postln;
	b[msg[1]] = Buffer.read(s, msg[2].asString);
}, '/newpath');

OSCdef(\listenSampleosc, {arg msg;
	Routine{
		var c = Condition(false);
		var buffer = Buffer.readChannel(s, msg[1].asString, channels: [0], action: {c.unhang});
		msg.postln;

		c.hang;
		Synth(\cue, [\buf, buffer]);

		Routine{
			(buffer.duration + 1).wait;
			c.unhang;
		}.play;

		c.hang;
		buffer.free;
		"liberato".postln;
	}.play
}, '/listenSample');

//~patList = [\kick, \hat, \snare, \clap, \pads, \bass];
//~synthList = [~voice, ~theremin];

Pbindef(\kickpattern).clear;
Pbindef(\hatpattern).clear;
Pbindef(\snarepattern).clear;
Pbindef(\clappattern).clear;

Pbindef(\kickpattern,
	\instrument, \kick,
	\trigger, Pseq(0!16,inf),
	\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
	\dur, 1/4,
	\buf, b.kick,
	//\rotate, 0,
	//\tumble, 0
);
Pbindef(\hatpattern,
	\instrument, \hat,
	\trigger, Pseq(0!16,inf),
	\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
	\dur, 1/4,
	\buf, b.hat,
	//\rotate, 0,
	//\tumble, 0
);
Pbindef(\snarepattern,
	\instrument, \snare,
	\trigger, Pseq(0!16,inf),
	\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
	\dur, 1/4,
	\buf, b.snare,
	//\rotate, 0,
	//\tumble, 0
);
Pbindef(\clappattern,
	\instrument, \clap,
	\trigger, Pseq(0!16,inf),
	\amp, Pif(Pkey(\trigger) > 0, 1, Rest()),
	\dur, 1/4,
	\buf, b.clap,
	//\rotate, 0,
	//\tumble, 0
);

Pbindef(\padspattern).clear;
Pbindef(\basspattern).clear;

//OSCs funzionanti

//variabili usate per \pads
/*var pads_ampBus, pads_cutoffBus, pads_panBus, pads_spaceBus, pads_chain;
var pads_ampFunc, pads_cutoffFunc, pads_panFunc, pads_spaceFunc;*/
pads_ampBus = Bus.control(s);
pads_cutoffBus = Bus.control(s);
pads_panBus = Bus.control(s);
pads_spaceBus = {Bus.control(s)} !3;

pads_spaceFunc = {
	Out.kr(pads_spaceBus[0], \rotate.kr(0));
	Out.kr(pads_spaceBus[1], \tumble.kr(0));
	Out.kr(pads_spaceBus[2], \distanza.kr(0));
}.play;

//Pbindef(\padspattern2, \rotate, spaceBus[0].asMap, \tumble, spaceBus[1].asMap).quant_(0); //forse inutile

~accordiPads = \rest!16; //note bass
OSCdef(\padsosc, {arg msg;
	//msg.postln;
	//msg[0] = path
	//msg[1] = posizione dell'accordo/nota
	//msg[2..] = freq (solo 4 valori magari)
	//size == 3: rest o set
	//se msg[2] == "rest"--> rest in msg[1]
	//set: msg[1] = control; msg[2] = value
	if(msg.size==2){
		switch(msg[1])
		{\play}{
			Pbindef(\padspattern, \instrument, \pads,
				\midinote, Pseq(~accordiPads,inf), //sicurezza in più, in caso passo seq vuota
				\dur, 1/4,
				\cutoff, 800,
				\detune, 0.1,
				\sustain, 1,
				\level, 1,
				\rel, 1,
				\pan, 0,
			);
			pads_chain = Pchain(Pbindef(\padspattern2), Pbindef(\padspattern)).play(t, quant: 4);
			//~chain.play(t, quant: 4);
			Pbindef(\padspattern2,
				\rotate, pads_spaceBus[0].asMap,
				\tumble, pads_spaceBus[1].asMap,
				\distanza, pads_spaceBus[2].asMap,

			).quant_(0);
		}
		{\free}{
			Pbindef(\padspattern).stop;
			Pbindef(\padspattern).clear;
			Pbindef(\padspattern2).stop;
			Pbindef(\padspattern2).clear;
			pads_chain.clear;//STOP PROBABILMENTE
			~accordiPads = \rest ! 16;
			//~accordiPads.postln;
		}
		{\pause}{
			//chain.quant_(0);
			pads_chain.pause;
			~pauseBeat = t.beats*4%16;
			~pauseBeat.postln;
		}
			{\resume}{
			//Pbindef(\padspattern).resume(t, Quant(4,(~pauseBeat+1).floor/4));
			// Pbindef(\padspattern2).resume(t, Quant(4, (~pauseBeat+1).floor/4));
			pads_chain.resume(t, Quant(4, (~pauseBeat+1).floor/4))
		}
		{\resumeB}{
			msg.postln;
			pads_chain.resume(t, Quant(4))
		}

			{
				//vuoto
				~accordiPads.put(msg[1], \rest);
				Pbindef(\padspattern,
					\instrument, \pads,
					\dur, 1/4,
					\midinote, Pseq(~accordiPads,inf)
				).quant_(4);
				//~accordiPads.postln;
			}
	};


	if(msg.size >= 3){//rest o set
		if(msg[2] == \rest){ //[\pads, pos, \rest]
			~accordiPads.put(msg[1], msg[2]);
			Pbindef(\padspattern,
				\instrument, \pads,
				\midinote, Pseq(~accordiPads,inf),
				\dur, 1/4
			).quant_(4);
			//~accordiPads.postln;
		}{//set

			switch(msg[1])
			{\amp}{//Pbindef(\padspattern, msg[1], msg[2].linlin(0,5,0,1)).quant_(1)

				if(msg[2] == \start){
					pads_ampFunc = {Out.kr(pads_ampBus, \value.kr(1).lag(0.1))}.play;
					Pbindef(\padspattern2, msg[1], pads_ampBus.asMap)/*.quant_(1)*/}
				{if(msg[2] == \stop){
					pads_ampFunc.free
				}{
					pads_ampFunc.set(\value, msg[2].linlin(-0.1,1.1,0,1))
				}
				}
			}

			//pattern buggato con asMap
			{\detune}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\padspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.1,1)).quant_(1)
				}{
					if(msg[2] == \stop){}
				}
			}
			{\rel}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\padspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.1,3)).quant_(1)
				}
			}
			{\level}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\padspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.1,1)).quant_(1)
				}
			}
			{\sustain}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\padspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.1,3)).quant_(1)
				}
			}
			{\cutoff}{//Pbindef(\padspattern, msg[1], msg[2].linexp(-2,2,800,18e3)).quant_(1)
				if(msg[2] == \start){
					pads_cutoffFunc = {Out.kr(pads_cutoffBus, \value.kr(1))}.play;
					Pbindef(\padspattern, msg[1], pads_cutoffBus.asMap).quant_(1)}
				{if(msg[2] == \stop){
					pads_cutoffFunc.free
				}{
					pads_cutoffFunc.set(\value, msg[2].linlin(-0.1,1.1 ,800,18e3))
				}
				}
			}
			//forse serve perché è la diffusione dello splay
			{\pan}{//Pbindef(\padspattern, msg[1], msg[2].linlin(-2,2,-1,1)).quant_(1)
				if(msg[2] == \start){
					pads_panFunc = {Out.kr(pads_panBus, \value.kr(0))}.play;
					Pbindef(\padspattern, msg[1], pads_panBus.asMap).quant_(1)}
				{if(msg[2] == \stop){
					pads_panFunc.free
				}{
					pads_panFunc.set(\value, msg[2].linlin(-0.1,1.1,-1,1))
				}
				}
			}
			{\rotate}{
				pads_spaceFunc.set(\rotate, msg[2].neg)
				// Pbindef(\padspattern2, msg[1], msg[2].neg)/*.quant_(1)*/
			}
			{\tumble}{
				pads_spaceFunc.set(\tumble, msg[2])
				// Pbindef(\padspattern2, msg[1], msg[2])/*.quant_(1)*/
			}
			{\distanza}{
				pads_spaceFunc.set(\distanza, msg[2].round(0.01).linlin(0.1, 10, 1, 0.1)
				)
			}

			{//defaulst: c'è un accordo msg[2..]
				//msg.postln;
				var quantValue;
				if(msg.last == \pino){quantValue = 0; "aaaaaaaaaaaa".postln}{quantValue = 4};
				msg.postln;
				~accordiPads.put(msg[1], msg[2..]);

				Pbindef(\padspattern,
					\instrument, \pads,
					\dur, 1/4,
					\midinote, Pseq(~accordiPads,inf)
				).quant_(/*4*/quantValue);
				//~accordiPads.postln;
			};

		}
	};

},'/pads');

OSCdef(\thereminosc, {
	|msg| msg.postln;
	if(msg.size==2){
		switch(msg[1])
		{\play}{~theremin = Synth(\theremin)}
		{\pause}{~theremin.run(false)}
		{\resume}{~theremin.run(true)}
		//{\free}{~theremin.set(\gate, 0)}
	}
	{
		if(msg.size==3){//set (vanno mappati)
			if((msg[2]!=\start).and(msg[2]!=\stop)){
				switch(msg[1])
				{\amp}{"controllo amp".postln; ~theremin.set(msg[1], msg[2].linlin(-0.1,1.1,0,1).postln)}
				{\gate}{~theremin.set(msg[1], msg[2])}
				{\carFreq}{~theremin.set(msg[1], msg[2].linexp(-0.1,1.1, 20,20e3))}
				{\modAmp}{~theremin.set(msg[1], msg[2].linexp(-0.1,1.1 ,1, 10e3))}
				{\rotate}{~theremin.set(msg[1], msg[2].neg)}
				{\tumble}{~theremin.set(msg[1], msg[2])}
				{\distanza}{~theremin.set(msg[1], msg[2].linlin(0.1, 6.5, 1, 0.01))}
		}};
}}, '/theremin');



//args : trigFreq, overlap, rate, pos, pan, amp,
OSCdef(\voiceosc, {
	|msg| msg.postln;
	if(msg.size==2){
		switch(msg[1])
		{\play}{~voice = Synth(\voice, [\buf, b.voice])}
		{\pause}{~voice.run(false)}
		{\resume}{~voice.run(true)}
		//{\free}{~voice.set(\gate, 0)} //per non fare più funzioni in unity
	}{
		if(msg.size==3){//set (vanno mappati)
			if((msg[2]!=\start).and(msg[2]!=\stop)){
				switch(msg[1])
				{\amp}{"controllo amp".postln; ~voice.set(msg[1], msg[2].linlin(-0.1,1.1,0,1).postln)}
				{\trigFreq}{~voice.set(msg[1], msg[2].linexp(-0.1,1.1, 1,200))}
				{\overlap}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0.1, 10))}
				{\rate}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0.5, 2))}
				{\rFreq}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0.2, 15))}
				{\rAmp}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , -0.3, 0.3))}
				{\pos}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0, 1))}
				{\diffusion}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0, 1))}
				// {\panFreq}{~voice.set(msg[1], msg[2].linlin(-0.1,1.1 , 0.1, 6))}
				{\gate}{~voice.set(msg[1], msg[2])}
				{\rotate}{~voice.set(msg[1], msg[2].neg)}
				{\tumble}{~voice.set(msg[1], msg[2])}
				{\distanza}{~voice.set(msg[1], msg[2].linlin(0.1, 6.5, 1, 0.01))}
		}};
}}, '/voice');

OSCdef(\pianoosc,{
	|msg|
	// msg[1] è la nota
	if(msg.size == 2 /*&& msg[1].isNumber*/){
		Synth(\piano, [\freq, msg[1]]);
	}
}, '/piano');

OSCdef(\tempoosc, {|msg|
	//setTempo --> [/tempo, value]
	//changeBar --> [/tempo, "bar"] --> t.sched(0, t.beats_(t.nextBar))
	msg.postln;
	t.schedAbs(t.nextBar, {t.tempo_(msg[1].linlin(0.0,1.0,60.0/60,160.0/60))})
},
'/tempo');




//bassOSC

// var bass_ampBus, bass_widthBus, bass_spaceBus, bass_chain;
// var bass_ampFunc, bass_widthFunc, bass_spaceFunc;
bass_ampBus = Bus.control(s);
bass_widthBus = Bus.control(s);
bass_spaceBus = {Bus.control(s)}!3;

bass_spaceFunc = {
	Out.kr(bass_spaceBus[0], \rotate.kr(0));
	Out.kr(bass_spaceBus[1], \tumble.kr(0));
	Out.kr(bass_spaceBus[2], \distanza.kr(0));

}.play;
//Pbindef(\basspattern2, \rotate, spaceBus[0].asMap, \tumble, spaceBus[1].asMap).quant_(0);
~accordiBasso = \rest!16; //note bass
//il bottone play (UI) istanzia il synth o lo distrugge

OSCdef(\bassosc, {arg msg;
	//msg.postln;

	if(msg.size==2){
		switch(msg[1])
		{\play}{Pbindef(\basspattern, \instrument, \bass,
			\midinote, Pseq(~accordiBasso - 12 ,inf), //sicurezza in più, in caso passo seq vuota
			\width, 0.7,
			\pan,0,
			\amp, 1,
			\rel, 3,
			\sweepDur, 0.7,
			\level,1,
			\sustain,1,
			// \rotate, 0,
			// \tumble,0
			);//.play(t);
		//Pbindef(\basspattern).quant_(4);
		bass_chain = Pchain(Pbindef(\basspattern2), Pbindef(\basspattern)).play(t, quant: 4);
		// chain.play(t, quant: 4);
		Pbindef(\basspattern2,
			\rotate, bass_spaceBus[0].asMap,
			\tumble, bass_spaceBus[1].asMap,
			\distanza, bass_spaceBus[2].asMap

		).quant_(0);
		}
		{\free}{
			Pbindef(\basspattern).stop;
			Pbindef(\basspattern).clear;
			Pbindef(\basspattern2).stop;
			Pbindef(\basspattern2).clear;
			bass_chain.clear;

			~accordiBasso = \rest ! 16;
			//~accordiBasso.postln;
			}
		{\pause}{
			/*Pbindef(\basspattern).quant_(0);
			Pbindef(\basspattern).pause;*/
			bass_chain.pause;
			~pauseBeat = t.beats*4%16;
			~pauseBeat.postln;}
		{\resume}{
			bass_chain.resume(t, Quant(4,(~pauseBeat+1).floor/4));
			// Pbindef(\basspattern).resume(t, Quant(4,(~pauseBeat+1).floor/4))

		}
		{\resumeB}{
			bass_chain.resume(t, Quant(0));

		}

		{
				//vuoto
				~accordiBasso.put(msg[1], \rest);
				Pbindef(\basspattern,
					\instrument, \bass,
					\midinote, Pseq(~accordiBasso - 12,inf),
					\dur, 1/4
				).quant_(4);
			//~accordiBasso.postln;
			}
	};
	if(msg.size >= 3){//else rest o set
		if(msg[2] == \rest){
			~accordiBasso.put(msg[1], msg[2]);
			Pbindef(\basspattern,
				\instrument, \bass,
				\midinote, Pseq(~accordiBasso - 12,inf),
				\dur, 1/4
			).quant_(4);
			//~accordiBasso.postln;
		}{//set

			switch(msg[1])
			{\amp}{
				if(msg[2] == \start){
					bass_ampFunc = {Out.kr(bass_ampBus, \value.kr(0).lag(0.1))}.play;
					Pbindef(\basspattern, msg[1], bass_ampBus.asMap).quant_(1);
				}{if(msg[2] == \stop)
					{bass_ampFunc.free}
					{bass_ampFunc.set(\value, msg[2].linlin(-0.1,1.1 ,0,1))}
				}
			}
			{\width}{
				if(msg[2] == \start){
					bass_widthFunc = {Out.kr(bass_widthBus, \value.kr(0).lag(0.1))}.play;
					Pbindef(\basspattern, msg[1], bass_widthBus.asMap).quant_(1);
				}{if(msg[2] == \stop)
					{bass_widthFunc.free}
					{bass_widthFunc.set(\value, msg[2].linlin(-0.1,1.1 ,0.1,1))}
				}
			}

			{\rel}{
				if((msg[2] != \start).and(msg[2] != \stop))
				{
					Pbindef(\basspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.6,3)).quant_(1)
				}
			}
			{\sweepDur}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\basspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.6,3)).quant_(1);
				}
			}
			{\sustain}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\basspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.6,3)).quant_(1);
				}
			}
			{\level}{
				if((msg[2] != \start).and(msg[2] != \stop)){
					Pbindef(\basspattern, msg[1], msg[2].linlin(-0.1,1.1 ,0.1,1)).quant_(1);
				}
			}
			{\rotate}{
				// Pbindef(\basspattern, msg[1], msg[2].neg).quant_(1)
				bass_spaceFunc.set(\rotate, msg[2].neg)
			}
			{\tumble}{
				bass_spaceFunc.set(\tumble, msg[2])
				// Pbindef(\basspattern, msg[1], msg[2]).quant_(1)
			}
			{\distanza}{
				bass_spaceFunc.set(\distanza, msg[2].round(0.01).linlin(0.1, 10, 1, 0.1))
				// Pbindef(\basspattern, msg[1], msg[2]).quant_(1)
			}

			{
				var quantValue;
				if(msg.last == \pino){quantValue = 0; "aaaaaaaaaaaa".postln}{quantValue = 4};
				msg.postln;
				~accordiBasso.put(msg[1], msg[2..]);
				Pbindef(\basspattern,
					\instrument, \bass,
					\midinote, Pseq(~accordiBasso - 12,inf),
					\dur, 1/4
				).quant_(/*4*/ quantValue);
				//~accordiBasso.postln;
			}
		}
	};
},'/bass');

g = Group(nil, \addToTail); //gruppo reverb e limiter
Synth(\limiter, target:g, addAction:\addAfter);
~reverb = nil;
~reverb = Synth(\reverb, target:g);
OSCdef(\reverbosc, {|msg|
	msg.postln;
	//~reverb ?? {~reverb = Synth(\reverb, target:g)};
	if(msg[2] > 0)
	{
		~reverb ?? {~reverb = Synth(\reverb, target:g)};
		~reverb.set(\drywet, msg[2])
	}{~reverb.free; ~reverb = nil;}
}, '/reverb');

// ~ascoltaPezzo = {~unity.sendMsg('/go'); t.beatsPerBar}
OSCdef(\piece, {
	|msg|
	msg.postln;
	switch(msg[1])
	{\parti}{~ascoltaPezzo = {~unity.sendMsg('/go'); t.beatsPerBar}}
	{\fine}{~ascoltaPezzo = {nil}};
	t.schedAbs(t.nextBar, {~ascoltaPezzo.()});
}, '/piece')
)
