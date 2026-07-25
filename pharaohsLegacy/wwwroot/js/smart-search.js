// Smart Search — بند 16
// بيدور على أي input بالـ id ده في الصفحة (زي مربع البحث في الـ Navbar جوه _Layout.cshtml)
// ولازم يكون جنبه <div id="smartSearchSuggestions"></div> فاضي عشان الـ dropdown يترسم فيه

(function () {
    const INPUT_ID = "smartSearchInput";
    const DROPDOWN_ID = "smartSearchSuggestions";
    const DEBOUNCE_MS = 300;
    const MIN_CHARS = 2;

    const TYPE_ICON = {
        pharaoh: "𓀀",
        temple: "𓉢",
        museum: "𓋹",
        god: "𓂀",
        artifact: "𓎟",
        dynasty: "𓊹"
    };

    const TYPE_ROUTE = {
        pharaoh: "/Pharaoh/Details/",
        temple: "/Temple/Details/",
        museum: "/Museum/Details/",
        god: "/God/Details/",
        artifact: "/Artifact/Details/",
        dynasty: "/Dynasty/Details/"
    };

    let debounceTimer = null;

    document.addEventListener("DOMContentLoaded", function () {
        const input = document.getElementById(INPUT_ID);
        const dropdown = document.getElementById(DROPDOWN_ID);
        if (!input || !dropdown) return; // الصفحة دي مفيهاش مربع بحث — ولا حاجة

        input.addEventListener("input", function () {
            const term = input.value.trim();
            clearTimeout(debounceTimer);

            if (term.length < MIN_CHARS) {
                dropdown.innerHTML = "";
                dropdown.style.display = "none";
                return;
            }

            debounceTimer = setTimeout(() => fetchSuggestions(term, dropdown), DEBOUNCE_MS);
        });

        // إقفال الـ dropdown لو دوس برا
        document.addEventListener("click", function (e) {
            if (!dropdown.contains(e.target) && e.target !== input) {
                dropdown.style.display = "none";
            }
        });
    });

    function fetchSuggestions(term, dropdown) {
        fetch(`/Home/SearchSuggestions?term=${encodeURIComponent(term)}`)
            .then(res => res.json())
            .then(items => renderDropdown(items, dropdown, term))
            .catch(() => {
                dropdown.innerHTML = "";
                dropdown.style.display = "none";
            });
    }

    function renderDropdown(items, dropdown, term) {
        if (!items || items.length === 0) {
            dropdown.innerHTML = "";
            dropdown.style.display = "none";
            return;
        }

        dropdown.innerHTML = items.map(item => {
            const icon = TYPE_ICON[item.type] || "🔍";
            const url = (TYPE_ROUTE[item.type] || "#") + item.id;
            return `
                <a href="${url}" class="smart-search-item"
                   onclick="fetch('/Home/TrackSearchClick',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},body:'query=${encodeURIComponent(term)}&resultType=${item.type}'})">
                    <span class="smart-search-icon">${icon}</span>
                    <span class="smart-search-name">${item.name}</span>
                </a>`;
        }).join("");

        dropdown.style.display = "block";
    }
})();
