console.log("POLL.JS LOADED");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/PollHub")
    .build();

connection.on("PollUpdated", function (options) {
    console.log("SIGNALR UPDATE", options);
    updateProgress(options);
});

connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error("SignalR error:", err));

// 🔥 VOTE VIA SIGNALR (GEEN FETCH MEER!)
function vote(optionId, pollId) {

    const cookieName = "voted_poll_" + pollId;

    if (document.cookie.includes(cookieName)) {
        alert("Je hebt al gestemd!");
        return;
    }

    document.cookie = cookieName + "=true; path=/";

    connection.invoke("Vote", pollId, optionId)
        .catch(err => console.error(err));
}

function updateProgress(options) {
    let totalVotes = options.reduce((sum, o) => sum + o.votes, 0);

    options.forEach(o => {
        const bar = document.querySelector(`.progress-bar[data-option-id='${o.id}']`);
        if (!bar) return;

        let percent = totalVotes === 0 ? 0 : Math.round((o.votes / totalVotes) * 100);

        bar.style.width = percent + "%";
        bar.innerText = `${o.text} (${percent}%)`;
    });

    window.addEventListener("load", () => {
        const pollId = document.querySelector(".optionBtn")?.getAttribute("onclick")?.match(/\d+/g)?.[1];

        const cookieName = "voted_poll_" + pollId;

        if (document.cookie.includes(cookieName)) {
            document.querySelectorAll(".optionBtn").forEach(btn => btn.disabled = true);
        }
    });

    connection.on("UpdateUserCount", function (count) {
        console.log("USERS:", count);
        document.getElementById("onlineUsers").innerText = count;
    });

}