$("#btnSave").on('click', function (event) {
    event.preventDefault();
    SaveChanges();
});

function SaveChanges() {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    const scheduleData = [];
    const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    for (const day of days) {
        const dayValue = $(`#Day_${day}`).val(); // Get Day value
        const medicId = $("#Id").val(); // Get MedicId value
        const timeStartId = `#TimeStart${day}`;
        const timeEndId = `#TimeEnd${day}`;
        const timeStartValue = $(TimeStartId).val(); // Get TimeStart value
        const timeEndValue = $(TimeEndId).val(); // Get TimeEnd value

        // Create and push schedule object for each day
        scheduleData.push({
            Day: dayValue,
            MedicId: medicId,
            TimeStart: timeStartValue,
            TimeEnd: timeEndValue
        });
    }

    // Convert scheduleData to JSON array
    const jsonData = JSON.stringify(scheduleData);
    $.ajax({
        url: "/Controller/Action", // Replace with your actual controller and action URL
        type: "POST",
        data: { scheduleData: jsonData },
        success: function (response) {
            // Handle successful response (e.g., display success message)
            console.log("Schedule data submitted successfully!");
        },
        error: function (error) {
            // Handle error response (e.g., display error message)
            console.error("Error submitting schedule data:", error);
        }
    });

    console.log(jsonData);
}