const hubConnection = new signalR.HubConnectionBuilder().withUrl('/console').build();

hubConnection.on("Receive", function (message) {
    let msgElem = document.createElement('span');
    msgElem.textContent = message;
    document.getElementById('homeLog').appendChild(msgElem);

    let homeLog = document.getElementById('homeLog');
    homeLog.scrollTop = homeLog.scrollHeight;
});

hubConnection.start().catch(function (err) {
    console.error(err.toString());
});

function startDownload() {
    $('#homeRun').removeClass('home_run');
    $('#homeRun').addClass('home_run_disabled');
    $('#homeRun').attr('disabled', 'disabled');
    $.ajax({
        url: '/Home/StartDownload',
        type: 'GET',
        success: function (data) {
            if (data === 'verificationRequested') {
                location.href = "/Home/ShowVerificationForm"
            }
            if (data === 'phoneNumberRequested') {
                location.href = "/Home/ShowPhoneNumberForm"
            }
            if (data === 'started') {              
                $('#homeStop').removeClass('home_stop_disabled');
                $('#homeStop').removeAttr('disabled');                
                $('#homeStop').addClass('home_stop'); 
                $('#phoneChange').removeClass('home_a');
                $('#phoneChange').addClass('home_a_disabled');
            }
            if (data.startsWith('error:')) {
                alert('Ошибка авторизации: ' + data.substring(6) + '\n\nПопробуйте еще раз\nПри неудачной попытке обратитесь к разработчику');
                $('#homeRun').removeClass('home_run_disabled');
                $('#homeRun').removeAttr('disabled');
                $('#homeRun').addClass('home_run');
            }
        },
        error: function (error) {
            console.error(error);
        }
    });
}

function startDownloadFromAnotherNumber() {
    $.ajax({
        url: '/Home/StartDownloadFromAnotherNumber',
        type: 'GET',
        success: function (data) {
            if (data === 'phoneNumberRequested') {
                location.href = "/Home/ShowPhoneNumberForm"
            }
        }
    });
}

function stopDownload() {
    $('#homeStop').removeClass('home_stop');
    $('#homeStop').addClass('home_stop_disabled');
    $('#homeStop').attr('disabled', 'disabled');
    $.ajax({
        url: '/Home/StopDownload',
        type: 'GET',
        success: function (data) {
            if (data === 'stopped') {
                $('#homeRun').removeClass('home_run_disabled');                
                $('#homeRun').removeAttr('disabled');
                $('#homeRun').addClass('home_run');    
                $('#phoneChange').removeClass('home_a_disabled');
                $('#phoneChange').addClass('home_a');
            }
        },
        error: function (error) {
            console.error(error);
        }
    });
}