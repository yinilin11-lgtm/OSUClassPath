const form = document.getElementById("agentForm");
const input = document.getElementById("agentInput");
const submitButton = document.getElementById("agentSubmit");
const messages = document.getElementById("agentMessages");
const statusLabel = document.getElementById("agentStatus");
const suggestionButtons = document.querySelectorAll("[data-agent-question]");

function appendMessage(role, text) {
    const message = document.createElement("div");
    message.className = `agent-message agent-message-${role}`;

    const avatar = document.createElement("div");
    avatar.className = "agent-avatar";
    avatar.textContent = role === "user" ? "You" : "AI";

    const bubble = document.createElement("div");
    bubble.className = "agent-bubble";
    bubble.textContent = text;

    message.appendChild(avatar);
    message.appendChild(bubble);
    messages.appendChild(message);
    messages.scrollTop = messages.scrollHeight;

    return message;
}

function setLoading(isLoading) {
    submitButton.disabled = isLoading;
    input.disabled = isLoading;
    statusLabel.textContent = isLoading ? "Thinking" : "Ready";
}

async function sendMessage(message) {
    const trimmedMessage = message.trim();
    if (!trimmedMessage) {
        return;
    }

    appendMessage("user", trimmedMessage);
    input.value = "";
    setLoading(true);

    const pendingMessage = appendMessage("assistant", "Checking the local course database...");

    try {
        const response = await fetch("/api/course-advisor/chat", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ message: trimmedMessage })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.detail || data.title || "AI advisor request failed.");
        }

        const contextNote = data.usedCourseContext
            ? ""
            : "\n\nNote: I could not find matching local course records, so this answer may need manual verification.";

        pendingMessage.querySelector(".agent-bubble").textContent = `${data.answer}${contextNote}`;
    } catch (error) {
        pendingMessage.querySelector(".agent-bubble").textContent =
            `I could not reach the AI advisor: ${error.message}`;
    } finally {
        setLoading(false);
        input.focus();
    }
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    await sendMessage(input.value);
});

suggestionButtons.forEach((button) => {
    button.addEventListener("click", async () => {
        input.value = button.dataset.agentQuestion || "";
        await sendMessage(input.value);
    });
});
