const form = document.getElementById("agentForm");
const input = document.getElementById("agentInput");
const submitButton = document.getElementById("agentSubmit");
const messages = document.getElementById("agentMessages");
const statusLabel = document.getElementById("agentStatus");

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

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    const message = input.value.trim();
    if (!message) {
        return;
    }

    appendMessage("user", message);
    input.value = "";
    setLoading(true);

    const pendingMessage = appendMessage("assistant", "思考中...");

    try {
        const response = await fetch("/api/agent/chat", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ message })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.detail || data.title || "AI agent request failed.");
        }

        const contextNote = data.usedCourseContext
            ? ""
            : "\n\n補充：這次沒有找到直接相關的本機課程資料，所以回答主要來自模型推論。";

        pendingMessage.querySelector(".agent-bubble").textContent = `${data.answer}${contextNote}`;
    } catch (error) {
        pendingMessage.querySelector(".agent-bubble").textContent =
            `目前無法取得 AI 回覆：${error.message}`;
    } finally {
        setLoading(false);
        input.focus();
    }
});
