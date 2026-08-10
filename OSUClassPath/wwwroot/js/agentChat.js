const form = document.getElementById("agentForm");
const agentPage = document.querySelector(".agent-page");
const input = document.getElementById("agentInput");
const submitButton = document.getElementById("agentSubmit");
const messages = document.getElementById("agentMessages");
const statusLabel = document.getElementById("agentStatus");
const suggestionButtons = document.querySelectorAll(".agent-suggestion");
const languageButtons = document.querySelectorAll("[data-agent-language]");
const conversationHistory = [];
const maxHistoryMessages = 10;
const plannerQueueStorageKey = "osuCoursePathPlannerQueue";
let currentLanguage = localStorage.getItem("agentLanguage") || "en";

const translations = {
    en: {
        navHome: "Home",
        navCourses: "Courses",
        navAdvisor: "Course Advisor",
        navStudents: "Students",
        navMyCourses: "My Courses",
        navPrivacy: "Privacy",
        eyebrow: "OSU CoursePath AI Advisor",
        heroTitle: "Plan CSE courses by requirement and track",
        heroCopy: "Ask about local catalog records, prerequisites, BS CSE requirements, technical electives, and focus areas such as AI, game development, software engineering, security, systems, and data.",
        datasetLabel: "Dataset",
        datasetValue: "133 local records",
        tracksLabel: "Tracks",
        tracksValue: "AI, Gaming, Security",
        statusLabel: "Status",
        ready: "Ready",
        thinking: "Thinking",
        suggestedTitle: "Suggested questions",
        suggestionAi: "AI course path",
        suggestionGame: "Game / graphics track",
        suggestionSecurity: "Security electives",
        suggestionCore: "Core requirements",
        chatTitle: "Course advisor chat",
        chatCopy: "Ask about course descriptions, prerequisites, BS CSE requirements, and academic plans.",
        welcome: "Hi, I am the OSU CoursePath AI advisor. Ask me about course information, prerequisites, BS CSE requirements, or planning ideas based on the local course catalog.",
        inputLabel: "Enter your question",
        inputPlaceholder: "Ask a course question, such as: What is CSE 2221 about?",
        send: "Send",
        userAvatar: "You",
        checking: "Checking the local course database...",
        missingContextNote: "Note: I could not find matching local course records, so this answer may need manual verification.",
        requestFailed: "AI advisor request failed.",
        unreachable: "I could not reach the AI advisor:"
        ,
        suggestedCoursesTitle: "Suggested courses",
        addToPlanner: "Add to Planner",
        addedToPlanner: "Added",
        openPlanner: "Open Planner",
        creditsLower: "credits"
    },
    zh: {
        navHome: "\u4e3b\u9801",
        navCourses: "\u8ab2\u7a0b",
        navAdvisor: "\u8ab2\u7a0b\u52a9\u7406",
        navStudents: "\u5b78\u751f\u8cc7\u6599",
        navMyCourses: "\u6211\u7684\u8ab2\u7a0b",
        navPrivacy: "\u96b1\u79c1",
        eyebrow: "OSU CoursePath AI \u9867\u554f",
        heroTitle: "\u4f9d\u7167\u7562\u696d\u8981\u6c42\u8207\u65b9\u5411\u898f\u5283 CSE \u8ab2\u7a0b",
        heroCopy: "\u4f60\u53ef\u4ee5\u8a62\u554f\u672c\u5730\u8ab2\u7a0b\u8cc7\u6599\u3001\u5148\u4fee\u8ab2\u3001BS CSE \u7562\u696d\u8981\u6c42\u3001technical electives\uff0c\u4ee5\u53ca AI\u3001\u904a\u6232\u958b\u767c\u3001\u8edf\u9ad4\u5de5\u7a0b\u3001\u8cc7\u5b89\u3001\u7cfb\u7d71\u3001\u8cc7\u6599\u7b49\u65b9\u5411\u3002",
        datasetLabel: "\u8cc7\u6599\u96c6",
        datasetValue: "133 \u7b46\u672c\u5730\u8cc7\u6599",
        tracksLabel: "\u65b9\u5411",
        tracksValue: "AI\u3001\u904a\u6232\u3001\u8cc7\u5b89",
        statusLabel: "\u72c0\u614b",
        ready: "\u5c31\u7dd2",
        thinking: "\u601d\u8003\u4e2d",
        suggestedTitle: "\u5efa\u8b70\u554f\u984c",
        suggestionAi: "AI \u8ab2\u7a0b\u8def\u7dda",
        suggestionGame: "\u904a\u6232 / \u5716\u5b78\u65b9\u5411",
        suggestionSecurity: "\u8cc7\u5b89\u9078\u4fee",
        suggestionCore: "\u6838\u5fc3\u8981\u6c42",
        chatTitle: "\u8ab2\u7a0b\u9867\u554f\u804a\u5929",
        chatCopy: "\u8a62\u554f\u8ab2\u7a0b\u4ecb\u7d39\u3001\u5148\u4fee\u8981\u6c42\u3001BS CSE \u7562\u696d\u8981\u6c42\u8207\u4fee\u8ab2\u898f\u5283\u3002",
        welcome: "\u4f60\u597d\uff0c\u6211\u662f OSU CoursePath AI \u9867\u554f\u3002\u4f60\u53ef\u4ee5\u6839\u64da\u672c\u5730\u8ab2\u7a0b\u8cc7\u6599\uff0c\u8a62\u554f\u8ab2\u7a0b\u8cc7\u8a0a\u3001\u5148\u4fee\u8981\u6c42\u3001BS CSE \u7562\u696d\u8981\u6c42\u6216\u4fee\u8ab2\u898f\u5283\u3002",
        inputLabel: "\u8f38\u5165\u4f60\u7684\u554f\u984c",
        inputPlaceholder: "\u8a62\u554f\u8ab2\u7a0b\u554f\u984c\uff0c\u4f8b\u5982\uff1aCSE 2221 \u662f\u4ec0\u9ebc\u8ab2\uff1f",
        send: "\u9001\u51fa",
        userAvatar: "\u4f60",
        checking: "\u6b63\u5728\u67e5\u8a62\u672c\u5730\u8ab2\u7a0b\u8cc7\u6599...",
        missingContextNote: "\u63d0\u9192\uff1a\u6211\u6c92\u6709\u627e\u5230\u5b8c\u5168\u7b26\u5408\u7684\u672c\u5730\u8ab2\u7a0b\u8cc7\u6599\uff0c\u6240\u4ee5\u9019\u500b\u56de\u7b54\u53ef\u80fd\u9700\u8981\u4eba\u5de5\u78ba\u8a8d\u3002",
        requestFailed: "AI \u9867\u554f\u8acb\u6c42\u5931\u6557\u3002",
        unreachable: "\u76ee\u524d\u7121\u6cd5\u9023\u4e0a AI \u9867\u554f\uff1a",
        suggestedCoursesTitle: "\u5efa\u8b70\u8ab2\u7a0b",
        addToPlanner: "\u52a0\u5230\u898f\u5283",
        addedToPlanner: "\u5df2\u52a0\u5165",
        openPlanner: "\u958b\u555f\u898f\u5283",
        creditsLower: "\u5b78\u5206"
    }
};

const suggestionQuestions = {
    en: {
        ai: "I want to focus on AI. Which CSE courses should I consider?",
        game: "Which courses fit a game development or graphics track?",
        security: "Which security technical electives are available?",
        core: "What are the core CSE courses and core choices?"
    },
    zh: {
        ai: "\u6211\u60f3\u8d70 AI \u65b9\u5411\uff0c\u6709\u54ea\u4e9b CSE \u8ab2\u7a0b\u53ef\u4ee5\u8003\u616e\uff1f",
        game: "\u54ea\u4e9b\u8ab2\u9069\u5408\u904a\u6232\u958b\u767c\u6216\u96fb\u8166\u5716\u5b78\u65b9\u5411\uff1f",
        security: "\u6709\u54ea\u4e9b\u8cc7\u5b89\u76f8\u95dc\u7684 technical electives\uff1f",
        core: "CSE core courses \u548c core choices \u6709\u54ea\u4e9b\uff1f"
    }
};

function t(key) {
    return translations[currentLanguage][key] || translations.en[key] || key;
}

function appendMessage(role, text) {
    const message = document.createElement("div");
    message.className = `agent-message agent-message-${role}`;

    const avatar = document.createElement("div");
    avatar.className = "agent-avatar";
    avatar.textContent = role === "user" ? t("userAvatar") : "AI";

    const bubble = document.createElement("div");
    bubble.className = "agent-bubble";
    if (role === "assistant") {
        bubble.innerHTML = renderAssistantMarkdown(text);
    } else {
        bubble.textContent = text;
    }

    message.appendChild(avatar);
    message.appendChild(bubble);
    messages.appendChild(message);
    messages.scrollTop = messages.scrollHeight;

    return message;
}

function appendSuggestedCourses(messageElement, courses) {
    if (!Array.isArray(courses) || courses.length === 0) {
        return;
    }

    const panel = document.createElement("div");
    panel.className = "agent-course-suggestions";

    const title = document.createElement("h3");
    title.textContent = t("suggestedCoursesTitle");
    panel.appendChild(title);

    const grid = document.createElement("div");
    grid.className = "agent-course-suggestion-grid";

    courses.slice(0, 6).forEach((rawCourse) => {
        const course = normalizeSuggestedCourse(rawCourse);
        const card = document.createElement("article");
        card.className = "agent-course-card";
        card.innerHTML = `
            <div class="course-card-top">
                <span class="course-code">${escapeHtml(course.code)}</span>
                <span class="credit-chip">${course.credits} ${escapeHtml(t("creditsLower") || "credits")}</span>
            </div>
            <h4>${escapeHtml(course.title)}</h4>
            <p>${escapeHtml(course.category)}${course.track ? ` · ${escapeHtml(course.track)}` : ""}</p>
            <div class="agent-course-card-actions">
                <button type="button">${escapeHtml(t("addToPlanner"))}</button>
                <a href="/Planner">${escapeHtml(t("openPlanner"))}</a>
            </div>
        `;

        const addButton = card.querySelector("button");
        addButton.addEventListener("click", () => {
            queueCourseForPlanner(course.code);
            addButton.textContent = t("addedToPlanner");
            addButton.disabled = true;
        });

        grid.appendChild(card);
    });

    panel.appendChild(grid);
    messageElement.insertAdjacentElement("afterend", panel);
    messages.scrollTop = messages.scrollHeight;
}

function normalizeSuggestedCourse(course) {
    return {
        code: course.courseCode || course.CourseCode || "",
        title: course.title || course.Title || "",
        credits: course.credits || course.Credits || 0,
        category: course.category || course.Category || "",
        track: course.track || course.Track || "",
        prerequisiteText: course.prerequisiteText || course.PrerequisiteText || ""
    };
}

function queueCourseForPlanner(courseCode) {
    const normalizedCode = String(courseCode || "").trim().toUpperCase();
    if (!normalizedCode) {
        return;
    }

    let queuedCourses = [];
    try {
        queuedCourses = JSON.parse(localStorage.getItem(plannerQueueStorageKey)) || [];
    } catch {
        queuedCourses = [];
    }

    if (!queuedCourses.includes(normalizedCode)) {
        queuedCourses.push(normalizedCode);
    }

    localStorage.setItem(plannerQueueStorageKey, JSON.stringify(queuedCourses));
}

function escapeHtml(value) {
    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function renderInlineMarkdown(value) {
    return escapeHtml(value)
        .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
        .replace(/`(.+?)`/g, "<code>$1</code>");
}

function renderAssistantMarkdown(markdown) {
    const lines = markdown.trim().split(/\r?\n/);
    const html = [];
    let listOpen = false;

    function closeList() {
        if (listOpen) {
            html.push("</ul>");
            listOpen = false;
        }
    }

    for (const rawLine of lines) {
        const line = rawLine.trim();

        if (!line) {
            closeList();
            continue;
        }

        if (/^-{3,}$/.test(line)) {
            closeList();
            html.push("<hr>");
            continue;
        }

        const heading = line.match(/^(#{2,4})\s+(.+)$/);
        if (heading) {
            closeList();
            const level = Math.min(heading[1].length, 4);
            html.push(`<h${level}>${renderInlineMarkdown(heading[2])}</h${level}>`);
            continue;
        }

        const numberedItem = line.match(/^\d+\.\s+(.+)$/);
        const bulletItem = line.match(/^[-*]\s+(.+)$/);
        const item = numberedItem?.[1] || bulletItem?.[1];
        if (item) {
            if (!listOpen) {
                html.push("<ul>");
                listOpen = true;
            }
            html.push(`<li>${renderInlineMarkdown(item)}</li>`);
            continue;
        }

        closeList();
        html.push(`<p>${renderInlineMarkdown(line)}</p>`);
    }

    closeList();
    return html.join("");
}

function setLoading(isLoading) {
    submitButton.disabled = isLoading;
    input.disabled = isLoading;
    if (statusLabel) {
        statusLabel.textContent = isLoading ? t("thinking") : t("ready");
    }
}

async function sendMessage(message) {
    const trimmedMessage = message.trim();
    if (!trimmedMessage) {
        return;
    }

    appendMessage("user", trimmedMessage);
    const historyForRequest = conversationHistory.slice(-maxHistoryMessages);
    conversationHistory.push({ role: "user", content: trimmedMessage });
    input.value = "";
    setLoading(true);

    const pendingMessage = appendMessage("assistant", t("checking"));

    try {
        const response = await fetch("/api/course-advisor/chat", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                message: `${trimmedMessage}\n\nPlease answer in ${currentLanguage === "zh" ? "Traditional Chinese" : "English"}.`,
                history: historyForRequest
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.detail || data.title || t("requestFailed"));
        }

        const contextNote = data.usedCourseContext
            ? ""
            : `\n\n${t("missingContextNote")}`;

        const assistantAnswer = `${data.answer}${contextNote}`;
        pendingMessage.querySelector(".agent-bubble").innerHTML = renderAssistantMarkdown(assistantAnswer);
        appendSuggestedCourses(pendingMessage, data.suggestedCourses || data.SuggestedCourses);
        conversationHistory.push({ role: "assistant", content: assistantAnswer });
        trimConversationHistory();
    } catch (error) {
        const errorMessage = `${t("unreachable")} ${error.message}`;
        pendingMessage.querySelector(".agent-bubble").innerHTML = renderAssistantMarkdown(errorMessage);
        conversationHistory.push({ role: "assistant", content: errorMessage });
        trimConversationHistory();
    } finally {
        setLoading(false);
        input.focus();
    }
}

function trimConversationHistory() {
    if (conversationHistory.length > maxHistoryMessages) {
        conversationHistory.splice(0, conversationHistory.length - maxHistoryMessages);
    }
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    await sendMessage(input.value);
});

suggestionButtons.forEach((button) => {
    button.addEventListener("click", async () => {
        const suggestion = button.dataset.agentSuggestion;
        input.value = suggestionQuestions[currentLanguage][suggestion] || suggestionQuestions.en[suggestion] || "";
        await sendMessage(input.value);
    });
});

function applyLanguage(language) {
    currentLanguage = translations[language] ? language : "en";
    localStorage.setItem("agentLanguage", currentLanguage);

    document.documentElement.lang = currentLanguage === "zh" ? "zh-Hant" : "en";

    agentPage?.querySelectorAll("[data-i18n]").forEach((element) => {
        element.textContent = t(element.dataset.i18n);
    });

    agentPage?.querySelectorAll("[data-i18n-placeholder]").forEach((element) => {
        element.placeholder = t(element.dataset.i18nPlaceholder);
    });

    languageButtons.forEach((button) => {
        const isActive = button.dataset.agentLanguage === currentLanguage;
        button.classList.toggle("active", isActive);
        button.setAttribute("aria-pressed", isActive ? "true" : "false");
    });

    if (statusLabel) {
        statusLabel.textContent = input.disabled ? t("thinking") : t("ready");
    }
}

languageButtons.forEach((button) => {
    button.addEventListener("click", () => {
        applyLanguage(button.dataset.agentLanguage);
    });
});

window.addEventListener("osuCoursePathLanguageChanged", (event) => {
    applyLanguage(event.detail?.language || "en");
});

applyLanguage(currentLanguage);
