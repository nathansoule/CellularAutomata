var rng = (function () {
	var RandomNumberGenerator = require("rng-js");
	var WellRNG = require("well-rng");

	var WellImplementation = new WellRNG();
	return new RandomNumberGenerator(function () {
		return WellImplementation.random();
	});
})();


//Initialize global constants
const vMAX = 5;
const pFAULT = 0.1;
const pSLOW = 0.5;
const ROAD_LENGTH = 50;
const DENSITY = 0.2;

/**
 * This takes two indicies and swaps them. While the JSDoc says String or Number for input, what is really meant is any sort of index for the object being called on. It then swaps the valeus of the two indicies.
 *
 * @param {String|Number} x
 * @param {String|Number} y
 * @returns {Array} this
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
 * @returns {[Object]} Objects have fields: vel {Number}, pos {Number}, wait {Boolean}
 */
var INITIALIZE = function () {
	//postcondition: output[] is an array of length DENSITY*ROADLENGTH + 1 cars. Velocities initialized to 1.
	//The last item is a copy of the first car, to simplify velocity update loops.*/
	var output = [];
	var num_of_cars = DENSITY * ROAD_LENGTH;
	for (var i = 0; i < num_of_cars; i++) {
		output.push({ vel: 1, wait: false });
	}

	//make last element refrence to first element
	output.push(output[0]);

	//tmp_position initialized to array of the road locations, then shuffled
	var start_positions = new Array();
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

//Updates each car's velocity
var Look = function (current, next) {
	var dist = (next.pos - current.pos) % ROAD_LENGTH;
	var spd = current.vel;
	var spd_next = next.vel;
	//stopped and already waited to advance after space opened up
	if (spd === 0 && dist > 1 && current.wait === true) {
		current.vel = 1;
		current.wait = false;
	}
	//stopped and space opens up, haven't yet waited
	else if (spd === 0 && dist > 1 && current.wait === false) {
		var check1 = rng.random();
		current.wait = check1 < pSLOW;
		current.vel = current.wait ? 0: 1; //advances speed only if wait check fails
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
	else if (spd < dist && dist <= 2*spd && spd >= spd_next + 4) {
		current.vel = spd - 2;
	}
	//slowing down, next car is far but moving slower than current
	else if (spd < dist && dist <= 2*spd && spd_next + 2 <= spd && spd <= spd_next + 3) {
		current.vel = spd - 1;
	}
	//next car is far enough ahead or moving fast enough, current accelerates towards speed limit
	else if (spd < vMAX && dist > spd + 1) {
		current.vel++;
	}
	//random deceleration, simulating driver error and road conditions
	if (current.vel > 0) {
		var check2 = rng.random();
		current.vel -= (check2 < pFAULT) 1 : 0;
	}
};

//Moves all cars forward according to their new velocity
var Move = function (road) {
	for (var i = 0; i < ROAD_LENGTH - 1; i++) {
		road[i].pos = (road[i].pos + road[i].vel) % ROAD_LENGTH;
	}
};

var Time_Step = function () {



};

