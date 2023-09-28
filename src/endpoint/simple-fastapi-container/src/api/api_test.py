import random

from fastapi.testclient import TestClient

from .main import app

client = TestClient(app)


def test_generate_name():
    # create fastapi test client
    random.seed(1)
    response = client.get("/generate_name")
    assert response.status_code == 200
    assert response.json()["name"] == "Belton"


def test_generate_name_params():
    random.seed(1)
    response = client.get("/generate_name?starts_with=n")
    assert response.status_code == 200
    assert response.json()["name"] == "Nancy"


def test_generate_name_params_upper():
    random.seed(1)
    response = client.get("/generate_name?starts_with=NE")
    assert response.status_code == 200
    assert response.json()["name"] == "Newell"
