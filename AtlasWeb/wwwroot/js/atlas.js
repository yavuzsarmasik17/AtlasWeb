
document.addEventListener("DOMContentLoaded", function () {

    const reduceMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    ).matches;

    if (reduceMotion || !("IntersectionObserver" in window)) {
        return;
    }

    const elements = document.querySelectorAll(
        ".atlas-section, .atlas-card, .atlas-contact"
    );

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("atlas-visible");
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.08,
        rootMargin: "0px 0px 40px 0px"
    });

    elements.forEach(function (element) {
        element.classList.add("atlas-reveal");
        observer.observe(element);
    });

});

/* ATLAS WMS - EKRAN TANITIM SEKMELERİ */

document.addEventListener("DOMContentLoaded", function () {

    const tabs = document.querySelectorAll(".atlas-preview-tab");
    const image = document.getElementById("atlasPreviewImage");
    const placeholder = document.getElementById("atlasPreviewPlaceholder");
    const title = document.getElementById("atlasPreviewTitle");

    if (!tabs.length || !image || !placeholder || !title) {
        return;
    }

    const screens = {
        "mal-kabul": {
            file: "/images/wms/mal-kabul.png",
            title: "Mal Kabul Yönetimi",
            alt: "ATLAS WMS Mal Kabul ekranı"
        },
        "mal-cikis": {
            file: "/images/wms/mal-cikis.png",
            title: "Mal Çıkış Yönetimi",
            alt: "ATLAS WMS Mal Çıkış ekranı"
        },
        "muhasebe": {
            file: "/images/wms/muhasebe.png",
            title: "Muhasebe Yönetimi",
            alt: "ATLAS WMS Muhasebe ekranı"
        }
    };

    let currentKey = "";

    function showScreen(key) {

        const screen = screens[key];
        if (!screen) return;

        currentKey = key;

        tabs.forEach(function (tab) {
            const active = tab.dataset.preview === key;
            tab.classList.toggle("active", active);
            tab.setAttribute("aria-pressed", String(active));
        });

        image.hidden = true;
        placeholder.hidden = false;
        title.textContent = screen.title;

        image.onload = function () {
            if (currentKey !== key) return;
            image.hidden = false;
            placeholder.hidden = true;
        };

        image.onerror = function () {
            if (currentKey !== key) return;
            image.hidden = true;
            placeholder.hidden = false;
        };

        image.alt = screen.alt;
        image.src = screen.file;
    }

    tabs.forEach(function (tab) {
        tab.addEventListener("click", function () {
            showScreen(tab.dataset.preview);
        });
    });

    showScreen("mal-kabul");

});
