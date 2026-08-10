const plannerData = window.osuPlannerData || { courses: [], terms: [] };
const plannerStorageKey = "osuCoursePathPlanner";
const plannerQueueStorageKey = "osuCoursePathPlannerQueue";
const completedCourseCodes = new Set(
    (plannerData.completedCourseCodes || plannerData.CompletedCourseCodes || []).map(normalizeCode)
);

const coursesByCode = new Map(
    plannerData.courses.map((course) => [normalizeCode(course.CourseCode || course.courseCode), normalizeCourse(course)])
);

const defaultTerms = plannerData.terms.map((term) => ({
    name: term.DisplayName || term.displayName,
    recommendedCredits: term.RecommendedCredits || term.recommendedCredits || 15,
    courses: (term.CourseCodes || term.courseCodes || []).filter((code) => coursesByCode.has(normalizeCode(code)))
}));

let plannerTerms = loadPlanner();

const grid = document.getElementById("plannerGrid");
const alerts = document.getElementById("plannerAlerts");
const searchInput = document.getElementById("plannerCourseSearch");
const courseOptions = document.getElementById("plannerCourseOptions");
const termSelect = document.getElementById("plannerTermSelect");
const addButton = document.getElementById("plannerAddButton");
const resetButton = document.getElementById("plannerResetButton");
const totalCreditsElement = document.getElementById("plannerTotalCredits");
const warningCountElement = document.getElementById("plannerWarningCount");

function normalizeCourse(course) {
    return {
        id: course.Id || course.id,
        code: course.CourseCode || course.courseCode,
        title: course.Title || course.title,
        credits: course.Credits || course.credits || 0,
        category: course.Category || course.category || "",
        track: course.Track || course.track || "",
        prerequisiteText: course.PrerequisiteText || course.prerequisiteText || ""
    };
}

function normalizeCode(code) {
    return String(code || "").trim().toUpperCase().replace(/\s+/g, " ");
}

function loadPlanner() {
    const saved = localStorage.getItem(plannerStorageKey);

    if (!saved) {
        return structuredClone(defaultTerms);
    }

    try {
        const parsed = JSON.parse(saved);
        if (Array.isArray(parsed) && parsed.length > 0) {
            return parsed;
        }
    } catch {
    }

    return structuredClone(defaultTerms);
}

function savePlanner() {
    localStorage.setItem(plannerStorageKey, JSON.stringify(plannerTerms));
}

function applyQueuedCourses() {
    let queuedCourses = [];
    try {
        queuedCourses = JSON.parse(localStorage.getItem(plannerQueueStorageKey)) || [];
    } catch {
        queuedCourses = [];
    }

    const validQueuedCourses = queuedCourses
        .map(normalizeCode)
        .filter((code) => coursesByCode.has(code));

    if (validQueuedCourses.length === 0) {
        return;
    }

    validQueuedCourses.forEach((courseCode) => {
        plannerTerms.forEach((term) => {
            term.courses = term.courses.filter((code) => code !== courseCode);
        });

        findBestTermForCourse(courseCode).courses.push(courseCode);
    });

    localStorage.removeItem(plannerQueueStorageKey);
    savePlanner();
}

function findBestTermForCourse(courseCode) {
    const course = coursesByCode.get(courseCode);
    const firstTermWithRoom = plannerTerms.find((term) => {
        const credits = term.courses.reduce((sum, code) => sum + (coursesByCode.get(code)?.credits || 0), 0);
        return credits + (course?.credits || 0) <= Math.max(term.recommendedCredits, 15);
    });

    return firstTermWithRoom || plannerTerms[plannerTerms.length - 1];
}

function setupOptions() {
    courseOptions.innerHTML = "";
    coursesByCode.forEach((course) => {
        const option = document.createElement("option");
        option.value = `${course.code} - ${course.title}`;
        courseOptions.appendChild(option);
    });

    termSelect.innerHTML = "";
    plannerTerms.forEach((term, index) => {
        const option = document.createElement("option");
        option.value = index.toString();
        option.textContent = term.name;
        termSelect.appendChild(option);
    });
}

function renderPlanner() {
    grid.innerHTML = "";

    const warnings = findWarnings();
    const warningMap = new Map();

    warnings.forEach((warning) => {
        const current = warningMap.get(warning.courseCode) || [];
        current.push(warning.message);
        warningMap.set(warning.courseCode, current);
    });

    plannerTerms.forEach((term, termIndex) => {
        const termCard = document.createElement("article");
        termCard.className = "planner-term";

        const credits = term.courses.reduce((sum, code) => sum + (coursesByCode.get(code)?.credits || 0), 0);

        termCard.innerHTML = `
            <div class="planner-term-header">
                <div>
                    <span>${term.name}</span>
                    <strong>${credits} credits</strong>
                </div>
                <small>${term.recommendedCredits} recommended</small>
            </div>
            <div class="planner-term-list"></div>
        `;

        const list = termCard.querySelector(".planner-term-list");

        if (term.courses.length === 0) {
            const empty = document.createElement("p");
            empty.className = "planner-empty";
            empty.textContent = "No courses yet";
            list.appendChild(empty);
        }

        term.courses.forEach((courseCode) => {
            const course = coursesByCode.get(courseCode);
            if (!course) {
                return;
            }

            const item = document.createElement("div");
            item.className = "planner-course";
            const courseWarnings = warningMap.get(course.code) || [];

            item.innerHTML = `
                <div>
                    <span class="course-code">${course.code}</span>
                    <h2>${course.title}</h2>
                    <p>${course.credits} credits · ${course.category}</p>
                    ${courseWarnings.map((message) => `<small class="planner-warning-chip">${message}</small>`).join("")}
                </div>
                <button type="button" aria-label="Remove ${course.code}">Remove</button>
            `;

            item.querySelector("button").addEventListener("click", () => {
                plannerTerms[termIndex].courses = plannerTerms[termIndex].courses.filter((code) => code !== course.code);
                savePlanner();
                renderPlanner();
            });

            list.appendChild(item);
        });

        grid.appendChild(termCard);
    });

    totalCreditsElement.textContent = plannerTerms
        .flatMap((term) => term.courses)
        .reduce((sum, code) => sum + (coursesByCode.get(code)?.credits || 0), 0);

    warningCountElement.textContent = warnings.length;
    renderWarnings(warnings);
}

function renderWarnings(warnings) {
    alerts.innerHTML = "";

    if (warnings.length === 0) {
        const clear = document.createElement("div");
        clear.className = "planner-alert planner-alert-clear";
        clear.textContent = "No prerequisite timing issues found in this draft plan.";
        alerts.appendChild(clear);
        return;
    }

    warnings.forEach((warning) => {
        const item = document.createElement("div");
        item.className = "planner-alert";
        item.textContent = `${warning.courseCode}: ${warning.message}`;
        alerts.appendChild(item);
    });
}

function findWarnings() {
    const termIndexByCourse = new Map();
    plannerTerms.forEach((term, termIndex) => {
        term.courses.forEach((courseCode) => termIndexByCourse.set(courseCode, termIndex));
    });

    completedCourseCodes.forEach((courseCode) => {
        if (!termIndexByCourse.has(courseCode)) {
            termIndexByCourse.set(courseCode, -1);
        }
    });

    const warnings = [];

    plannerTerms.forEach((term, termIndex) => {
        term.courses.forEach((courseCode) => {
            const course = coursesByCode.get(courseCode);
            if (!course) {
                return;
            }

            const prereqCodes = extractCourseCodes(course.prerequisiteText)
                .filter((code) => code !== course.code && coursesByCode.has(code));

            prereqCodes.forEach((prereqCode) => {
                const prereqTermIndex = termIndexByCourse.get(prereqCode);

                if (prereqTermIndex === undefined) {
                    warnings.push({
                        courseCode: course.code,
                        message: `${prereqCode} is listed as a prerequisite but is not in this plan.`
                    });
                    return;
                }

                if (prereqTermIndex >= termIndex) {
                    warnings.push({
                        courseCode: course.code,
                        message: `${prereqCode} should be planned before this term.`
                    });
                }
            });
        });
    });

    return warnings;
}

function extractCourseCodes(text) {
    const matches = String(text || "").match(/\b[A-Z]{2,12}\s?\d{4}[A-Z]?\b/g) || [];
    return [...new Set(matches.map(normalizeCode))];
}

function findCourseFromInput(value) {
    const directCode = normalizeCode(value.split("-")[0]);
    if (coursesByCode.has(directCode)) {
        return coursesByCode.get(directCode);
    }

    const typed = value.toLowerCase();
    return [...coursesByCode.values()].find((course) =>
        course.code.toLowerCase() === typed ||
        `${course.code} - ${course.title}`.toLowerCase() === typed
    );
}

addButton.addEventListener("click", () => {
    const course = findCourseFromInput(searchInput.value);
    const termIndex = Number(termSelect.value);

    if (!course || Number.isNaN(termIndex)) {
        searchInput.focus();
        return;
    }

    plannerTerms.forEach((term) => {
        term.courses = term.courses.filter((code) => code !== course.code);
    });

    plannerTerms[termIndex].courses.push(course.code);
    searchInput.value = "";
    savePlanner();
    renderPlanner();
});

resetButton.addEventListener("click", () => {
    plannerTerms = structuredClone(defaultTerms);
    savePlanner();
    setupOptions();
    renderPlanner();
});

setupOptions();
applyQueuedCourses();
renderPlanner();
