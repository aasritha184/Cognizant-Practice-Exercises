console.log("Welcome to the Community Portal");

let seats = 20;
const eventName = "Tech Meetup";
console.log(`${eventName} has ${seats} seats`);

class Event {
  constructor(name, seats) {
    this.name = name;
    this.seats = seats;
  }
  checkAvailability() {
    return this.seats > 0;
  }
}

let events = [
  new Event("Tech Meetup", 5),
  new Event("Music Night", 0),
  new Event("Baking Workshop", 3),
];

function filterAvailable(list) {
  return list.filter(e => e.checkAvailability());
}
console.log(filterAvailable(events).map(e => e.name));

document.getElementById("regForm").addEventListener("submit", function (e) {
  e.preventDefault();
  const name = e.target.elements.name.value;
  const type = e.target.elements.eventType.value;
  document.getElementById("confirmMsg").textContent = `Thanks ${name}, registered for ${type}!`;
});

document.getElementById("phone").addEventListener("blur", function (e) {
  const valid = /^\d{10}$/.test(e.target.value);
  document.getElementById("phoneError").textContent = e.target.value && !valid ? "Invalid phone number" : "";
});

document.getElementById("feedbackText").addEventListener("keyup", function (e) {
  document.getElementById("charCount").textContent = `${e.target.value.length} characters`;
});

document.getElementById("galleryImg").addEventListener("dblclick", function (e) {
  e.target.style.width = e.target.style.width === "300px" ? "150px" : "300px";
});

document.getElementById("promoVideo").addEventListener("canplay", function () {
  document.getElementById("videoMsg").textContent = "Video ready to play";
});

document.getElementById("saveBtn").addEventListener("click", function () {
  const pref = document.getElementById("prefType").value;
  localStorage.setItem("prefType", pref);
  document.getElementById("prefMsg").textContent = `Saved: ${pref}`;
});

document.getElementById("clearBtn").addEventListener("click", function () {
  localStorage.removeItem("prefType");
  document.getElementById("prefMsg").textContent = "Cleared";
});

window.addEventListener("DOMContentLoaded", function () {
  const saved = localStorage.getItem("prefType");
  if (saved) document.getElementById("prefType").value = saved;
});

document.getElementById("geoBtn").addEventListener("click", function () {
  const msg = document.getElementById("geoMsg");
  navigator.geolocation.getCurrentPosition(
    pos => { msg.textContent = `Lat ${pos.coords.latitude.toFixed(2)}, Lng ${pos.coords.longitude.toFixed(2)}`; },
    err => { msg.textContent = "Could not get location"; }
  );
});

async function loadEvents() {
  const res = await new Promise(resolve => setTimeout(() => resolve(events), 500));
  console.log("Loaded events:", res.map(e => e.name));
}
loadEvents();
