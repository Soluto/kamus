// Most of the code is based on this SO answer: https://stackoverflow.com/a/29382205/4792970
package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
	"strings"
)

type DecryptRequest struct {
	EncryptedData string `json:"data"`
}

var (
	version        = "SetByCI"
	buildTimestamp = "SetByCI"
)

func main() {
	fmt.Println("Decryptor version", version)
	fmt.Println("Built on", buildTimestamp)

	if len(os.Args) < 3 {
		fmt.Println("Usage: decryptor <source-file> <target-file>")
		os.Exit(1)
	}

	fmt.Println("Decryptor starting")

	// Creating the maps for JSON
	m := map[string]interface{}{}

	f, _ := os.Open(os.Args[1])

	// Use bufio.NewReader to get a Reader.
	// ... Then use ioutil.ReadAll to read the entire content.
	reader := bufio.NewReader(f)
	content, _ := ioutil.ReadAll(reader)

	// Parsing/Unmarshalling JSON encoding/json
	err := json.Unmarshal([]byte(content), &m)

	if err != nil {
		panic(err)
	}
	parseMap(m)

	b, err := json.Marshal(m)
	if err != nil {
		fmt.Println("error:", err)
	}

	file, err := os.OpenFile(
		os.Args[2],
		os.O_WRONLY|os.O_TRUNC|os.O_CREATE,
		0666,
	)
	if err != nil {
		panic(err)
	}
	defer file.Close()

	_, err = file.Write(b)
	if err != nil {
		panic(err)
	}

	fmt.Println("Decryptor run completed successfully")
}

func decrypt(encodedData string) string {

	fmt.Println(encodedData)
	f1, err := os.Open("/var/run/secrets/kubernetes.io/serviceaccount/token")

	if err != nil {
		panic(err)
	}

	// Use bufio.NewReader to get a Reader.
	// ... Then use ioutil.ReadAll to read the entire content.
	reader1 := bufio.NewReader(f1)
	token, err := ioutil.ReadAll(reader1)

	if err != nil {
		panic(err)
	}

	decryptRequest := &DecryptRequest{EncryptedData: encodedData}

	b := new(bytes.Buffer)
	json.NewEncoder(b).Encode(decryptRequest)

	client := &http.Client{}

	hamusteUrl := os.Getenv("HAMUSTE_URL")

	if hamusteUrl == "" {
		hamusteUrl = "http://hamuste.team-devops.svc.cluster.local/"
	}

	req, err := http.NewRequest("POST", hamusteUrl+"api/v1/decrypt", b)

	if err != nil {
		panic(err)
	}

	req.Header.Add("Content-Type", "application/json")
	req.Header.Add("Authorization", "Bearer "+string(token))

	res, err := client.Do(req)

	if err != nil {
		panic(err)
	}

	defer res.Body.Close()
	body, err := ioutil.ReadAll(res.Body)

	if res.StatusCode > 299 {
		fmt.Println("Response from Hamuste does not indicate success: " + res.Status)
		os.Exit(1)
	}

	return string(body)
}

func parseMap(aMap map[string]interface{}) {
	for key, val := range aMap {
		switch concreteVal := val.(type) {
		case map[string]interface{}:
			parseMap(val.(map[string]interface{}))
		case []interface{}:
			parseArray(val.([]interface{}))
		default:

			switch concreteVal.(type) {
			case string:
				if strings.Index(concreteVal.(string), "secure:") == 0 {
					aMap[key] = decrypt(strings.Split(concreteVal.(string), ":")[1])
				}
			default:
			}

		}
	}
}

func parseArray(anArray []interface{}) {
	for i, val := range anArray {
		switch concreteVal := val.(type) {
		case map[string]interface{}:
			fmt.Println("Index:", i)
			parseMap(val.(map[string]interface{}))
		case []interface{}:
			fmt.Println("Index:", i)
			parseArray(val.([]interface{}))
		default:
			fmt.Println("Index", i, ":", concreteVal)

		}
	}
}
