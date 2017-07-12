var rng = (function () {
	var RandomNumberGenerator = require("rng-js");
	var WellRNG = require("well-rng");

	var WellImplementation = new WellRNG();
	return new RandomNumberGenerator(() => WellImplementation.random());
})();
var Clone = require('clone');


//Initialize global constants
const vMAX = 5;
const pFAULT = 0.1;
const pSLOW = 0.2;
const ROAD_LENGTH = 100;
const DENSITY = 0.2;
const RAMP_TIME = 1000; //Run time to allow traffic to stabilize from starting position
const SAMPLE_TIME = 100; //Length of time after ramp up that will be recorded for measurement

/**
 * This takes two indicies and swaps them. While the JSDoc says String or Number for input, what is really meant is any sort of index for the object being called on. It then swaps the valeus of the two indicies.
 *
 * @param {String|Number} x
 * @param {String|Number} y
 * @returns {Object} this
 */
Object.prototype.swap = function (x, y) {
	var b = this[x];
	this[x] = this[y];
	this[y] = b;
	return this;
};

/**
 * Shuffles an array using Fisher-Yates shuffle and returns the input array.
 *
 * @param {Array} arr
 * @returns {Array}
 */
var shuffle = function (arr) {
	var k = arr.length;
	if (k < 3) {
		for (var i = 0; i < k; i++)
			arr.swap(i, rng.random(0, k));
	} else {
		for (var i = (k - 1); i > 0; i--) {
			var j = rng.random(0, i);
			arr.swap(i, j);
		}
	}
	return arr;
};
/**
 * Initializes a random road with DENSITY * ROAD_LENGTH cars.
 *
 * @returns {Object[]} Objects have fields: vel {Number}, pos {Number}, wait {Boolean}
 */
var INITIALIZE = function () {
	//postcondition: output[] is an array of length DENSITY*ROADLENGTH + 1 cars. Velocities initialized to 1.
	//The last item is a copy of the first car, to simplify velocity update loops.*/
	var output = [];
	var num_of_cars = DENSITY * ROAD_LENGTH;
	for (var i = 0; i < num_of_cars; i++) {
		output.push({vel: 1, wait: false});
	}

	//tmp_position initialized to array of the road locations, then shuffled
	var start_positions = [];
	for (i = 0; i < ROAD_LENGTH; i++) {
		start_positions.push(i);
	}

	shuffle(start_positions);

	start_positions.length = num_of_cars;

	//sorts the positions into ascending order
	start_positions.sort(function (a, b) {
		return a - b;
	});

	for (i = 0; i < num_of_cars; i++) {
		output[i].pos = start_positions[i];
	}
	return output;
};

/**
 * Initializes array for recording distance data
 * @param {Object[]} cars array of car objects, represents all data of current road
 * @returns {Object[]} Object has fields: plate {Number}, TotalDist {Number}
 */
function INITIALIZE_SAMPLE(cars) {
	var output = [];
	//selects num_of_cars * DENSITY cars for measurement, roughly equally spaced and stores their number in SAMPLE[]
	//plate denotes the car's location in the road array
	var num_of_cars = DENSITY * ROAD_LENGTH;
	var sample_size = Math.floor(num_of_cars * DENSITY);
	var spacing = Math.floor(num_of_cars / sample_size);
	for (var i = 0; i < sample_size; i++) {
		output.push({ plate: i * spacing, TotalDist: 0 });
	}
	return output;
}

/**
 * function moves all object on road
 * @param {Object[]} cars array of car objects, represents all data of current road
 * @param {Number} roadLength the length of the road
 */
var Move = function (cars, roadLength) {
	for (var i = 0; i < cars.length; i++) {
		cars[i].pos = (cars[i].pos + cars[i].vel) % roadLength;
	}
};

/**
 * Records the distance traveled by a sample of cars for one time step
 * @param {Object[]} SAMPLE an array of sample car objects, with index in 'cars' array and total distances
 * @param {Object[]} cars array of car objects, representing current road
 */
function Record (SAMPLE, cars) {
	//appends travel data for cars in SAMPLE[]
	for (i = 0; i < SAMPLE.length; i++) {
		SAMPLE[i].TotalDist += cars[SAMPLE[i].plate].vel;
	}
}

/**
 *
 * @param {Object[]} startRoadState Expects a function that generates a road object. Road object format is documented in README
 * @param {Number} vMax Maximum speed of any car
 * @param {Number} pFault Probability to randomly slow down on a given time step. This simulates driver error and differing road conditions
 * @param {Number} pSlow Probability to delay starting for one time step when a car is stopped and space opens up
 * @param {Number} roadLength Length of road
 * @param {Number} steps Length of time
 * @param {Boolean} measure Whether current step will be recorded in SAMPLE, using function Record()
 */
function Run(startRoadState, vMax, pFault, pSlow, roadLength, steps, measure) {
	var cars = Clone(startRoadState, false);
	var numOfCars = cars.length;

	for (var s = 0; s < steps; s++) {
		for (var i = 0; i < numOfCars; i++) {
			//sets new speed on each car
			setSpeed(cars[i], cars[(i + 1) % numOfCars], vMax, pFault, pSlow, roadLength);
		}
		//moves car by new speed
		Move(cars, roadLength);
		if (measure === true) {
			Record(SAMPLE, cars);
		}
	}
	return cars;
}

/**
 * @param {Object} current Is a car
 * @param {Object} next Is the next car
 * @param {Number} vMax maximum velocity of a car
 * @param {Number} pFault Probability to randomly slow down on a given time step. This simulates driver error and differing road conditions
 * @param {Number} pSlow Probability to delay starting for one time step when a car is stopped and space opens up
 * @param {Number} roadLength Length of road
 */
function setSpeed(current, next, vMax, pFault, pSlow, roadLength) {
	var dist = ((next.pos - current.pos) + roadLength) % roadLength;
	var spd = current.vel;
	var spd_next = next.vel;
	//stopped and already waited to advance after space opened up
	if (spd === 0 && dist > 1 && current.wait) {
		current.vel = 1;
		current.wait = false;
	}
	//stopped and space opens up, haven't yet waited
	else if (spd === 0 && dist > 1 && !current.wait) {
		var check1 = rng.random();
		current.wait = check1 < pSlow;
		current.vel = current.wait ? 0 : 1; //advances speed only if wait check fails
	}
	//slowing down when next car is very close or moving faster than current
	else if (dist <= spd && (spd < spd_next || spd <= 2)) {
		current.vel = dist - 1;
	}
	//slowing down when next car is slower or same speed as current, or close and current car is moving fast
	else if (dist <= spd) {
		current.vel = ((dist - 1) < (spd - 2)) ? dist - 1 : spd - 2;
	}
	//slowing down when next car is far but stopped or moving much slower than current
	else if (spd < dist && dist <= 2 * spd && spd >= spd_next + 4) {
		current.vel = spd - 2;
	}
	//slowing down, next car is far but moving slower than current
	else if (spd < dist && dist <= 2 * spd && spd_next + 2 <= spd && spd <= spd_next + 3) {
		current.vel = spd - 1;
	}
	//next car is far enough ahead or moving fast enough, current accelerates towards speed limit
	else if (spd < vMax && dist > spd + 1) {
		current.vel++;
	}
	//random deceleration, simulating driver error and road conditions
	if (current.vel > 0) {
		var check2 = rng.random();
		current.vel -= (check2 < pFault) ? 1 : 0;
	}
}


var history = [INITIALIZE()];
var SAMPLE = INITIALIZE_SAMPLE(history[0]);
while (history.length < 25) {
	history.push(Run(history[history.length - 1], vMAX, pFAULT, pSLOW, ROAD_LENGTH, 1, false));
}
var message = 'jumping ' + RAMP_TIME + ' steps now';
history.push(message);
history.push(Run(history[history.length - 2], vMAX, pFAULT, pSLOW, ROAD_LENGTH, RAMP_TIME, false));
while (history.length < (26 + SAMPLE_TIME)) {
	history.push(Run(history[history.length - 1], vMAX, pFAULT, pSLOW, ROAD_LENGTH, 1, true));
}
print(history, ROAD_LENGTH);
print_dist_data(SAMPLE, ROAD_LENGTH, SAMPLE_TIME);

/**
 * Prints all data passed in
 * @param {Object[][]} data an array of road states and strings, strings are used as messages to be printed
 * @param {Number} roadLen length of the road
 */
function print(data, roadLen) {
	dataLen = data.length;
	for (var i = 0; i < dataLen; i++) {
		if (data[i].constructor === Array) {
			var current = [];
			for (var j = 0; j < data[i].length; j++) {
				current[data[i][j].pos] = data[i][j].vel;
			}
			var output = '';
			for (var j = 0; j < roadLen; j++) {
				if (typeof current[j] !== 'number')
					output += '_';
				else
					output += current[j];
			}
			console.log(output);
		} else if (typeof data[i] === 'string') {
			console.log(data[i]);
		}
	}
}

/**
 * Prints record of Total distances and avg speeds for steps 1000 - 1050
 * @param {Object[][]} data an array of positions and total distances
 * @param {Number} roadLen length of road 
 * @param {Number} sampleTime number of time steps that were recorded
 */
function print_dist_data(data, roadLen, sampleTime) {
	dataLen = data.length;
	console.log('Now printing distance and average velocity data for last ');
	console.log(sampleTime);
	console.log(' time steps.');
	for (var i = 0; i < dataLen; i++) {
		var avg_vel = data[i].TotalDist / sampleTime;
		var output = '';
		output += 'Car #';
		output += data[i].plate;
		output += ' traveled ';
		output += data[i].TotalDist;
		output += ' units, at an average velocity of ';
		output += avg_vel;
		console.log(output);
	}
}

