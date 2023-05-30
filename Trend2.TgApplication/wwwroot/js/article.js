function handleHeaderClick(event) {
    var field = event.target.getAttribute('data-field');
    document.getElementById('sortField').value = field;

    var dir = document.getElementById('sortDirection').value;
    if (dir === "true")
        document.getElementById('sortDirection').value = false;
    else
        document.getElementById('sortDirection').value = true;

    var form = document.getElementById('articleForm');
    $("#pubDate").val('');
    form.submit();
}

document.getElementById('headerId').addEventListener("click", handleHeaderClick);
document.getElementById('headerPubDate').addEventListener("click", handleHeaderClick);

document.getElementById("articlePageSelect").addEventListener("change", function () {
    var selectedPage = this.value;
    if (selectedPage) {
        var urlParams = new URLSearchParams(window.location.search);
        urlParams.set("page", selectedPage);
        var newUrl = window.location.pathname + "?" + urlParams.toString();
        window.location.href = newUrl;
    }
});

document.getElementById("tgListUnder").addEventListener("change", function () {
    var selectedValue = this.value;
    if (selectedValue) {
        var urlParams = new URLSearchParams(window.location.search);
        urlParams.set("pageSize", selectedValue);
        var newUrl = window.location.pathname + "?" + urlParams.toString();
        window.location.href = newUrl;
    }
});

var articleTitles = document.getElementsByClassName('article_first');

for (let i = 0; i < articleTitles.length; i++) {
    articleTitles[i].addEventListener("click", function (event) {
        var bodyElement = event.target.nextSibling;

        while (!bodyElement.classList)
            bodyElement = bodyElement.nextSibling;

        bodyElement.classList.toggle("article_hidden");
    }, false);
}

// Получение значения из поля ввода для поиска
var searchText = document.getElementById('articleHighlightInput').value;

// Функция для выделения текста внутри элемента
function highlightText(element, searchText) {
    searchText = searchText.trim();
    if (searchText == '') return;
    var innerHTML = element.innerHTML;
    var regex = new RegExp(searchText, 'gi');
    innerHTML = innerHTML.replace(regex, function (match) {
        return '<span style="background-color: yellow">' + match + '</span>';
    });
    element.innerHTML = innerHTML;
}

// Получение всех элементов с классом "article_text" и выделение искомого текста
var articleTextElements = document.getElementsByClassName('for_highlight');
console.log(articleTextElements);
for (var i = 0; i < articleTextElements.length; i++) {
    highlightText(articleTextElements[i], searchText);
}

// Получение всех элементов с id "articleHighlightBody" и выделение искомого текста
//var articleBodyElements = document.querySelectorAll('#articleHighlightBody');
//for (var i = 0; i < articleBodyElements.length; i++) {
//    highlightText(articleBodyElements[i], searchText);
//}