import csv
import yaml

# load the YAML file
for i in range(0,5000):
    with open(f'./Moore/stats-{i}.yaml') as f:
        data = yaml.safe_load(f)

    # extract the variables we're interested in
    variables = [
        'Metodo',
        'Fitness',
        'Pa',
        'vt',
        'vs',
        'A',
        'Pe',
        'E',
        'Et',
        'Es',
        'we',
        'wa',
        'Tempo de execucao'
    ]
    data['Metodo'] = data.pop('MÃ©todo')
    data['Tempo de execucao'] = data.pop('Tempo de execuÃ§Ã£o')
    values = [data[var] for var in variables]

    # write the values to a CSV file
    with open('output-Moore.csv', 'a', newline='') as f:
        writer = csv.writer(f)
        # writer.writerow(variables)
        writer.writerow(values)
