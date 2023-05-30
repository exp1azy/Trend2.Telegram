function handleHeaderClick(event) {
    var field = event.target.getAttribute('data-field');
    document.getElementById('sortField').value = field;

    var dir = document.getElementById('sortDirection').value;
    if (dir === "true")
        document.getElementById('sortDirection').value = false;
    else
        document.getElementById('sortDirection').value = true;

    var form = document.getElementById('indexForm');
    form.submit();
}

document.getElementById('headerId').addEventListener("click", handleHeaderClick);
document.getElementById('headerTitle').addEventListener("click", handleHeaderClick);
document.getElementById('headerCreated').addEventListener("click", handleHeaderClick);
document.getElementById('headerUpdated').addEventListener("click", handleHeaderClick);
document.getElementById('headerSite').addEventListener("click", handleHeaderClick);
document.getElementById('headerCount').addEventListener("click", handleHeaderClick);