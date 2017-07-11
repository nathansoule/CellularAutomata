var rng = (function () {
	var RandomNumberGenerator = require("rng-js");
	var WellRNG = require("well-rng");

	var WellImplementation = new WellRNG();
	return new RandomNumberGenerator(() => WellImplementation.random());
})();
var Clone = require('clone');


//Initialize global constants
const maxVelocity = 5;
const pFAULT = 0.1;
const pSLOW = 0.2;
const ROAD_LENGTH = 50;

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
var INITIALIZE = function (density, isDensity=true) {
	//postcondition: output[] is an array of length DENSITY*ROADLENGTH + 1 cars. Velocities initialized to 1.
	//The last item is a copy of the first car, to simplify velocity update loops.*/
	var output = [];
	var num_of_cars = isDensity ? Math.round(density * ROAD_LENGTH) : density;
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
 *
 * @param {Object[]} startRoadState Expects a function that generates a road object. Road object format is documented in README
 * @param {Number} vMax Maximum speed of any car
 * @param {Number} pFault Probability to randomly slow down on a given time step. This simulates driver error and differing road conditions
 * @param {Number} pSlow Probability to delay starting for one time step when a car is stopped and space opens up
 * @param {Number} roadLength Length of road
 * @param {Number} steps Length of time
 */
function Run(startRoadState, vMax, pFault, pSlow, roadLength, steps) {
	var cars = Clone(startRoadState, false);
	var numOfCars = cars.length;

	for (var s = 0; s < steps; s++) {
		for (var i = 0; i < numOfCars; i++) {
			//sets new speed on each car
			setSpeed(cars[i], cars[(i + 1) % numOfCars], vMax, pFault, pSlow, roadLength);
		}
		//moves car by new speed
		Move(cars, roadLength);
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

for (var i = 2; i < ROAD_LENGTH; i ++) {
	var history = [Run(INITIALIZE(i, false), maxVelocity, pFAULT, pSLOW, ROAD_LENGTH, 1000)];
	for (var j = 1; j < 250; j++) {
		history.push(Run(history[history.length - 1], maxVelocity, pFAULT, pSLOW, ROAD_LENGTH, 1000));
	}
	console.log(i + ' : ' + passPoint(history, ROAD_LENGTH) + '\t' + totalMovement(history));
}

function passPoint(data, roadLen) {
	var output = 0;
	for (var i = data.length-1; i >= 0; i--) {
		var current = data[i];
		for (var j = current.length-1; j >= 0; j--) {
			if (current[j].vel + current[j].pos >= roadLen) {
				output++;
			}
		}
	}
	return output;
}

function totalMovement(data) {
	var output = 0;
	for (var i = data.length-1; i >= 0; i--) {
		var current = data[i];
		for (var j = current.length - 1; i >= 0; i--) {
			output += current[j].vel;
		}
	}
	return output;
}


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



