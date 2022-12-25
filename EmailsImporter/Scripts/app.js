'use strict';

var app = {

    fetchGoogleEmails: function () {

        const btnId = '#btnImportGmail';
        const btnDefaultTxt = 'Import from Gmail';
        const defaultErrorMsg = 'Error occurred while fetching emails';

        showMessage('Retrieving emails from Gmail', 'info');
        changeButtonState(btnId, 'Importing...', true);

        const successCallback = (response) => {
            changeButtonState(btnId, btnDefaultTxt, false);

            switch (response.Type) {
                case 'Ok':
                    if (response.RedirectUri) {
                        window.location.href = response.RedirectUri;
                    }
                    else {
                        showMessage('Emails retrieved successfully!', 'success');
                        app.bindRows(response.Data);
                    }
                    break;

                case 'Error':
                    showMessage(response.ErrorMessage, 'error');
                    break;
            }
        }

        const errorCallback = (jqXhr, status, error) => {
            console.error(jqXhr);
            changeButtonState(btnId, btnDefaultTxt, false);

            const errorMsg = formatErrorMessage(jqXhr, error);
            const msg = errorMsg ? errorMsg : defaultErrorMsg;
            showMessage(msg, 'error');
        };

        ajaxCall('GET', '/Google/GetEmails', successCallback, errorCallback, null);
    },

    bindRows: function(data) {

        $.each (data, function( index, item ) {

            const row = `<tr>
                         <td>${item.Date}</td>
                         <td>${item.From}</td>
                         <td>${item.Subject}</td>
                         <td>${item.Attachments.length}</td>
                         </tr>`;

            $('#tblGmailEmails tr:last').after(row);
        });
    }
}