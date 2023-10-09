#include <pico/cyw43_arch.h>
#include "Bluetooth.h"
#include "../IO/CurrentIO.h"
#include "../UserInteractions.h"
#include "etl/queue.h"


#ifdef BOARD_PICO_W
#include "btstack.h"
#include "pico/cyw43_arch.h"
#include "pico/btstack_cyw43.h"
#include "pc.h"

#define HEARTBEAT_PERIOD_MS 10

static btstack_timer_source_t heartbeat;
static btstack_packet_callback_registration_t hci_event_callback_registration;

#endif

#define APP_AD_FLAGS 0x06
static uint8_t adv_data[] = {
        // Flags general discoverable
        0x02, BLUETOOTH_DATA_TYPE_FLAGS, APP_AD_FLAGS,
        // Name
        0x17, BLUETOOTH_DATA_TYPE_COMPLETE_LOCAL_NAME, 'P', 'i', 'c', 'o', 'C', 'o', 'n', 't', 'r', 'o', 'l', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
        //0x03, BLUETOOTH_DATA_TYPE_COMPLETE_LIST_OF_16_BIT_SERVICE_CLASS_UUIDS, 0x1a, 0x18,
        0x03, BLUETOOTH_DATA_TYPE_INCOMPLETE_LIST_OF_16_BIT_SERVICE_CLASS_UUIDS, 0x10, 0xff,
};
static const uint8_t adv_data_len = sizeof(adv_data);
bool leMsgNotificationsEnabled;
hci_con_handle_t con_handle;
static bool isInitialized;
static etl::queue<etl::string<20>, 100> messages;


[[noreturn]]
void HaltAndBlinkSos(etl::unique_ptr<IO::Output::LED> led)
{
    auto ptr = led.get();
    while(true)
    {
        UserInteractions::SOS(ptr);
        sleep_ms(2000);
    }
}

uint16_t att_read_callback(hci_con_handle_t connection_handle, uint16_t att_handle, uint16_t offset, uint8_t * buffer, uint16_t buffer_size) {
    UNUSED(connection_handle);

    if (att_handle == ATT_CHARACTERISTIC_95C26871_655A_11EE_B1A8_108EAB4F90E5_01_VALUE_HANDLE && !messages.empty())
    {
        etl::string<20> buf;
        messages.pop_into(buf);
        auto cStr = buf.c_str();
        return att_read_callback_handle_blob((const uint8_t*)cStr, 20, offset, buffer, buffer_size);
    }
    return 0;
}

int att_write_callback(hci_con_handle_t connection_handle, uint16_t att_handle, uint16_t transaction_mode, uint16_t offset, uint8_t *buffer, uint16_t buffer_size) {
    UNUSED(connection_handle);
    UNUSED(att_handle);
    UNUSED(transaction_mode);
    UNUSED(offset);
    UNUSED(buffer);
    UNUSED(buffer_size);

    if (att_handle == ATT_CHARACTERISTIC_95C26871_655A_11EE_B1A8_108EAB4F90E5_01_CLIENT_CONFIGURATION_HANDLE){
        leMsgNotificationsEnabled = little_endian_read_16(buffer, 0) == GATT_CLIENT_CHARACTERISTICS_CONFIGURATION_NOTIFICATION;
        con_handle = connection_handle;
        if (leMsgNotificationsEnabled) {
            att_server_request_can_send_now_event(con_handle);
        }
    }
//    if (att_handle != ATT_CHARACTERISTIC_ORG_BLUETOOTH_CHARACTERISTIC_TEMPERATURE_01_CLIENT_CONFIGURATION_HANDLE)
//        return 0;
//    leMsgNotificationsEnabled = little_endian_read_16(buffer, 0) == GATT_CLIENT_CHARACTERISTICS_CONFIGURATION_NOTIFICATION;
//    con_handle = connection_handle;
//    if (leMsgNotificationsEnabled) {
//        att_server_request_can_send_now_event(con_handle);
//    }
    return 0;
}



void packet_handler(uint8_t packet_type, uint16_t channel, uint8_t *packet, uint16_t size) {
    UNUSED(size);
    UNUSED(channel);
    bd_addr_t local_addr;
    if (packet_type != HCI_EVENT_PACKET) return;

    uint8_t event_type = hci_event_packet_get_type(packet);
    switch(event_type){
        case BTSTACK_EVENT_STATE: {
            if (btstack_event_state_get_state(packet) != HCI_STATE_WORKING)
                return;


            gap_local_bd_addr(local_addr);
            printf("BTstack up and running on %s.\n", bd_addr_to_str(local_addr));


            // setup advertisements
            uint16_t adv_int_min = 800;
            uint16_t adv_int_max = 800;
            uint8_t adv_type = 0;
            bd_addr_t null_addr;
            memset(null_addr, 0, 6);
            gap_advertisements_set_params(adv_int_min, adv_int_max, adv_type, 0, null_addr, 0x07, 0x00);
            assert(adv_data_len <= 31); // ble limitation
            gap_advertisements_set_data(adv_data_len, (uint8_t *) adv_data);
            gap_advertisements_enable(1);

            break;
        }
        case HCI_EVENT_DISCONNECTION_COMPLETE:
            leMsgNotificationsEnabled = 0;
            break;
        case ATT_EVENT_CAN_SEND_NOW:

            if (!messages.empty())
            {
                etl::string<20> buf;
                messages.pop_into(buf);
                auto cStr = buf.c_str();

                att_server_notify(
                        con_handle,
                        ATT_CHARACTERISTIC_95C26871_655A_11EE_B1A8_108EAB4F90E5_01_VALUE_HANDLE,
                        (uint8_t*)cStr,
                        20);
            }
            break;
        default:
            break;
    }
}

static void heartbeat_handler(struct btstack_timer_source *ts) {

    if (leMsgNotificationsEnabled) {
        att_server_request_can_send_now_event(con_handle);
    }

    // Restart timer
    btstack_run_loop_set_timer(ts, HEARTBEAT_PERIOD_MS);
    btstack_run_loop_add_timer(ts);
}

void Bluetooth::Initialize(bool hasBattery) {
#ifdef BOARD_PICO_W

    if (cyw43_arch_init()) {
        printf("bluetooth init failed");
        panic("bluetooth init failed");
    }
    if (hasBattery) {
        (void)IO::Battery::GetBatteryVoltage();

        bool value = true;
        auto led = IO::GetDefaultLED();
        for (int i = 0; i < 6; i++) {
            led->Set(value);
            sleep_ms(1000);
            value = !value;
        }

        if (IO::Battery::GetBatteryVoltage() < 1.85) {
            HaltAndBlinkSos(etl::move(led));
        }

    }

    l2cap_init();
    sm_init();

    att_server_init(profile_data, att_read_callback, att_write_callback);

    hci_event_callback_registration.callback = &packet_handler;
    hci_add_event_handler(&hci_event_callback_registration);

    att_server_register_packet_handler(packet_handler);

    heartbeat.process = &heartbeat_handler;

    btstack_run_loop_set_timer(&heartbeat, HEARTBEAT_PERIOD_MS);
    btstack_run_loop_add_timer(&heartbeat);

    // turn on bluetooth!
    hci_power_control(HCI_POWER_ON);

    isInitialized = true;
#endif
}

bool Bluetooth::IsInitialized() {
    return isInitialized;
}

void Bluetooth::PushMessage(etl::string<20> buffer) {
    messages.push(buffer);
}
